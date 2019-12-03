using System;
using System.Reflection;
using Harmony;
using UnityEngine;

namespace ToggleModule
{
    public static class Main
    {
        public static void Load()
        {
            try
            {
                var buildermodule = new VehicleToggleModulePrefab();
                buildermodule.Patch();
                var truckbuildermodule = new SeaTruckToggleModulePrefab();
                truckbuildermodule.Patch();
                HarmonyInstance.Create("MrPurple6411.ToggleModule").PatchAll(Assembly.GetExecutingAssembly());
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
            if (techType == VehicleToggleModulePrefab.TechTypeID && added)
            {
                var control = __instance.gameObject.GetOrAddComponent<VehicleToggleModule>();
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
            if (techType == SeaTruckToggleModulePrefab.TechTypeID && added)
            {
                var control = __instance.gameObject.GetOrAddComponent<SeaTruckToggleModule>();
                control.moduleSlotID = slotID + 1;
                return;
            }
        }
    }
}
