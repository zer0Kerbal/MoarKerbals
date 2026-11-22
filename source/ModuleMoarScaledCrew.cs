using System;
using System.Globalization;
using System.Linq;
using UnityEngine;
using MoarKerbals.Core;

namespace MoarKerbals
{
    // This owns instance capacity rather than relying on TweakScale's editor clamp or rounding.
    // Prefab capacity is the largest supported size so stock loading/KIS allocate all possible seats.
    public class ModuleMoarScaledCrew : PartModule
    {
        [KSPField] public int nominalSeats = 3;
        [KSPField] public double nominalScale = 2.5;
        [KSPField] public int maximumSeats = 10;
        [KSPField(isPersistant = true)] public double lastKnownScale = 2.5;
        [KSPField(guiName = "Crew capacity", guiActive = true, guiActiveEditor = true)] public string capacityDisplay = "";
        private bool ready;
        private bool failed;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            // Keep the loading prefab at maximumSeats so KIS creates all seat inventories.
            if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight) Apply(lastKnownScale, false);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            ready = HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight;
            if (ready) Synchronize();
        }

        public void LateUpdate()
        {
            if (ready && !failed && (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)) Synchronize();
        }

        private void Synchronize()
        {
            try
            {
                var tweak = part.Modules.Cast<PartModule>().FirstOrDefault(m => m.moduleName == "TweakScale");
                double scale = nominalScale;
                if (tweak != null && tweak.Fields["currentScale"] != null)
                    scale = Convert.ToDouble(tweak.Fields["currentScale"].GetValue(tweak), CultureInfo.InvariantCulture);
                if (scale <= 0) return; // TweakScale has not initialised yet.
                Apply(scale, true);
            }
            catch (Exception error)
            {
                failed = true;
                Logging.DLogError("Scaled crew capacity unavailable: " + error);
            }
        }

        private void Apply(double scale, bool editorManifest)
        {
            if (part == null) return;
            int calculated = Rules.VolumeCapacity(nominalSeats, scale, nominalScale);
            int desired = Math.Min(maximumSeats, calculated);
            if (part.protoModuleCrew != null)
                desired = Rules.PreserveOccupiedSeats(Math.Max(desired, part.protoModuleCrew.Count), part.protoModuleCrew.Select(k => k.seatIdx));
            PartCrewManifest manifest = null;
            if (editorManifest && HighLogic.LoadedSceneIsEditor && ShipConstruction.ShipManifest != null)
            {
                manifest = ShipConstruction.ShipManifest.GetPartCrewManifest(part.craftID);
                if (manifest != null && manifest.partCrew != null)
                    desired = Rules.PreserveOccupiedSeats(desired, manifest.partCrew.Select((name, i) => string.IsNullOrEmpty(name) ? -1 : i));
            }
            part.CrewCapacity = desired;
            if (part.internalModel != null && part.internalModel.transform.localScale.x > 0)
                foreach (var seat in part.internalModel.seats)
                    if (!(seat is MoarPassengerSeat))
                    {
                        seat.kerbalScale = Vector3.one / part.internalModel.transform.localScale.x;
                        if (seat.kerbalRef != null) seat.kerbalRef.transform.localScale = seat.kerbalScale;
                    }
            lastKnownScale = scale;
            capacityDisplay = desired + (desired > Math.Min(maximumSeats, calculated) ? " (occupied seats retained)" : "");
            if (manifest != null && manifest.partCrew != null && manifest.partCrew.Length != desired)
            {
                var seats = Enumerable.Repeat(string.Empty, desired).ToArray();
                Array.Copy(manifest.partCrew, seats, Math.Min(manifest.partCrew.Length, seats.Length));
                manifest.partCrew = seats;
            }
        }

        public override string GetInfo() => string.Format(CultureInfo.InvariantCulture,
            "Volume-scaled crew capacity: {0} at {1} m; up to {2}. Includes matching seat and personal-inventory capacity. Occupied seats are retained when shrinking.",
            nominalSeats, nominalScale, maximumSeats);
    }

    // A real stock InternalSeat on a stock passenger-chair prop, not a duplicate reference to Seat1/2/3.
    public class MoarPassengerSeat : InternalSeat
    {
        [KSPField] public int capacityIndex;
        public override void OnAwake()
        {
            var anchor = internalProp.transform.Find("MoarPassengerAnchor");
            if (anchor == null)
            {
                anchor = new GameObject("MoarPassengerAnchor").transform;
                anchor.SetParent(internalProp.transform, false);
                anchor.localPosition = new Vector3(0, 0.35f, 0);
            }
            // Stock FindModelTransform searches the imported model, not our runtime anchor.
            seatTransformName = null;
            allowCrewHelmet = false;
            base.OnAwake();
            seatTransform = anchor;
            displayseatName = "Passenger seat " + (capacityIndex + 1);
            // KSP AddProp registers only modules named exactly InternalSeat, not subclasses.
            var model = internalProp.internalModel;
            if (model != null && !model.seats.Contains(this)) model.seats.Add(this);
        }

        public void LateUpdate()
        {
            var model = internalProp != null ? internalProp.internalModel : null;
            if (model == null || model.part == null) return;
            // Keep added chair props and Kerbals at human scale when the IVA shell is enlarged.
            float shellScale = model.transform.localScale.x;
            if (shellScale > 0) internalProp.transform.localScale = Vector3.one / shellScale;
            bool visible = capacityIndex < model.part.CrewCapacity;
            foreach (var renderer in internalProp.GetComponentsInChildren<Renderer>()) renderer.enabled = visible;
        }
    }
}
