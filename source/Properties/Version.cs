using System.Reflection;
[assembly: AssemblyFileVersion("1.3.15.0")]
[assembly: AssemblyVersion("1.3.15.0")]
[assembly: KSPAssembly("MoarKerbals", 1, 3, 15)]
namespace MoarKerbals
{
    public static class Version
    {
        public const int major = 1, minor = 3, patch = 15, build = 0;
        public const string Number = "1.3.15.0";
        public const string SText = Number;
#if DEBUG
        public const string Text = Number + " DEBUG";
#else
        public const string Text = Number;
#endif
    }
}