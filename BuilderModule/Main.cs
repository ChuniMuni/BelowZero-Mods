using System;
using System.Reflection;
using Harmony;
using UnityEngine;

namespace BuilderModule
{
    public static class Main
    {
        public static void Load()
        {
            try
            {
                var buildermodule = new BuilderModulePrefab();
                buildermodule.Patch();
                var truckbuildermodule = new SeatruckBuilderModulePrefab();
                truckbuildermodule.Patch();
                HarmonyInstance.Create("MrPurple6411.BuilderModule").PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }

    [HarmonyPatch(typeof(Vehicle))]
    [HarmonyPatch("OnUpgradeModuleChange")]
    internal class Vehicle_OnUpgradeModuleChange_Patch
    {
        [HarmonyPostfix]
        static void Postfix(Vehicle __instance, int slotID, TechType techType, bool added)
        {
            if (techType == BuilderModulePrefab.TechTypeID && added)
            {
                var control = __instance.gameObject.GetOrAddComponent<BuilderModule>();
                control.moduleSlotID = slotID;
                return;
            }
        }
    }

    [HarmonyPatch(typeof(SeaTruckUpgrades))]
    [HarmonyPatch("OnUpgradeModuleChange")]
    internal class SeaTruckUpgrades_OnUpgradeModuleChange_Patch
    {
        [HarmonyPostfix]
        static void Postfix(SeaTruckUpgrades __instance, int slotID, TechType techType, bool added)
        {
            if (techType == SeatruckBuilderModulePrefab.TechTypeID && added)
            {
                var control = __instance.gameObject.GetOrAddComponent<SeaTruckBuilderModule>();
                control.moduleSlotID = slotID+1;
                return;
            }
        }
    }
}
