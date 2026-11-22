using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

// Inspect the built DLL without loading Unity. Catch resource calls that bypass the shared path.
static class ResourceAudit
{
    public static void Verify(string path)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        var r = pe.GetMetadataReader();
        string TypeName(EntityHandle handle) => handle.IsNil ? "" : handle.Kind switch {
            HandleKind.TypeDefinition => r.GetString(r.GetTypeDefinition((TypeDefinitionHandle)handle).Name),
            HandleKind.TypeReference => r.GetString(r.GetTypeReference((TypeReferenceHandle)handle).Name),
            _ => ""
        };
        (string owner, string name) Target(EntityHandle handle)
        {
            if (handle.Kind == HandleKind.MethodSpecification) return Target(r.GetMethodSpecification((MethodSpecificationHandle)handle).Method);
            if (handle.Kind == HandleKind.MethodDefinition)
            { var m = r.GetMethodDefinition((MethodDefinitionHandle)handle); return (TypeName(m.GetDeclaringType()), r.GetString(m.Name)); }
            if (handle.Kind == HandleKind.MemberReference)
            { var m = r.GetMemberReference((MemberReferenceHandle)handle); return (TypeName(m.Parent), r.GetString(m.Name)); }
            return ("", "");
        }
        void Require(bool value, string message) { if (!value) throw new Exception(message); }
        var consumers = r.TypeDefinitions.Where(h => TypeName(r.GetTypeDefinition(h).BaseType) == "MoarKerbalsBase")
            .Select(h => TypeName(h)).OrderBy(n => n).ToArray();
        Require(consumers.SequenceEqual(new[] { "KerbalRecruitment", "KloneBay", "KuddleShack" }), "Ability module inventory changed: audit the new consumers.");
        var opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null)!)
            .ToDictionary(op => unchecked((ushort)op.Value));
        var references = new List<(string source, string method, string target, string name)>();
        foreach (var typeHandle in r.TypeDefinitions)
        {
            var type = r.GetTypeDefinition(typeHandle); string owner = TypeName(typeHandle);
            foreach (var methodHandle in type.GetMethods())
            {
                var method = r.GetMethodDefinition(methodHandle); string name = r.GetString(method.Name);
                Require(name != "get_UseVesselResources", "Per-module routing switch has returned.");
                if (owner == "RecipeResources" && name == ".ctor")
                    Require(method.GetParameters().Count(p => r.GetParameter(p).SequenceNumber > 0) == 1, "Resource adapter must not offer a per-module mode switch.");
                if (method.RelativeVirtualAddress == 0) continue;
                var il = pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes()!;
                for (int i = 0; i < il.Length;)
                {
                    ushort code = il[i++]; if (code == 0xfe) code = (ushort)(0xfe00 | il[i++]);
                    var op = opcodes[code];
                    if (op.OperandType == OperandType.InlineMethod)
                    {
                        var target = Target(MetadataTokens.EntityHandle(BitConverter.ToInt32(il, i)));
                        references.Add((owner, name, target.owner, target.name));
                    }
                    i += op.OperandType switch {
                        OperandType.InlineNone => 0,
                        OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                        OperandType.InlineVar => 2,
                        OperandType.InlineI8 or OperandType.InlineR => 8,
                        OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, i),
                        _ => 4
                    };
                }
            }
        }
        var native = references.Where(c => c.name == "RequestResource" || c.name == "requestResource" || c.name == "GetConnectedResourceTotals").ToArray();
        Require(native.Length == 2 && native.All(c => c.source == "RecipeResources"), "A gameplay path bypasses the shared resource adapter.");
        foreach (string consumer in consumers)
            Require(references.Any(c => c.source == consumer && c.target == "MoarKerbalsBase" && c.name == "ExecutePaid"), consumer + " bypasses shared payment.");
        Require(references.Any(c => c.source == "MoarKerbalsBase" && c.method == "CompletionBlocker" && c.target == "RecipeResources" && c.name == "Totals"), "Preflight does not use the resource adapter.");
        Require(references.Any(c => c.source == "MoarKerbalsBase" && c.method == "ExecutePaid" && c.target == "RecipeResources" && c.name == "Request"), "Payment/refund does not use the resource adapter.");
        Require(references.Any(c => c.source == "MoarKerbalsBase" && c.method == "ExecutePaid" && c.name == "CompletionBlocker"), "Shared payment does not preflight shortages.");
        Require(references.Any(c => c.source == "MoarKerbalsBase" && c.method == "NotifyCrewChanged" && c.target == "CrewInventoryRefresh" && c.name == "Refresh"), "Shared crew-change path does not refresh inventory menus.");
        foreach (string consumer in consumers)
            Require(references.Any(c => c.source == consumer && c.name == "NotifyCrewChanged"), consumer + " omits the shared crew-change notification.");
        Require(!r.AssemblyReferences.Any(h => r.GetString(r.GetAssemblyReference(h).Name) == "KIS"), "KIS must remain optional.");
        Require(references.Any(c => c.source == "KerbalCloning" && c.method == "Create" && c.target == "ProtoCrewMember" && c.name == "SetDefaultInventory"),
            "Attribute clones must receive fresh stock default EVA equipment.");
    }
}
