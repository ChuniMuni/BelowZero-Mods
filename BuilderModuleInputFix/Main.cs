using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BuilderModuleInputFix
{
    public class Main
    {
        public static void Load()
        {
            try
            {
                HarmonyInstance.Create("MrPurple6411.BuilderModuleInputFix").PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }

    [HarmonyPatch(typeof(Builder))]
    [HarmonyPatch("Update")]
    internal class Builder_Update_Patch
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            if (Player.main.GetVehicle() != null || Player.main.GetComponentInParent<SeaTruckSegment>() != null)
            {
                Builder.Initialize();
                Builder.canPlace = false;
                if (Builder.prefab == null)
                {
                    return true;
                }
                if (!Builder.CreateGhost())
                {
                    Builder.inputHandler.canHandleInput = false;
                }
                Builder.canPlace = Builder.UpdateAllowed();
                Transform transform = Builder.ghostModel.transform;
                transform.position = Builder.placePosition + Builder.placeRotation * Builder.ghostModelPosition;
                transform.rotation = Builder.placeRotation * Builder.ghostModelRotation;
                transform.localScale = Builder.ghostModelScale;
                Color value = (!Builder.canPlace) ? Builder.placeColorDeny : Builder.placeColorAllow;
                IBuilderGhostModel[] components = Builder.ghostModel.GetComponents<IBuilderGhostModel>();
                for (int i = 0; i < components.Length; i++)
                {
                    components[i].UpdateGhostModelColor(Builder.canPlace, ref value);
                }
                Builder.ghostStructureMaterial.SetColor(ShaderPropertyID._Tint, value);

                ErrorMessage.AddMessage("in vehicle");
                return false;
            }
            else
            {
                ErrorMessage.AddMessage("not in vehicle");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Constructable))]
    [HarmonyPatch("Construct")]
    internal class Construct_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(Constructable __instance)
        {
            if (Player.main.GetVehicle() != null || Player.main.GetComponentInParent<SeaTruckSegment>() !=null)
            {
                var thisVehicle = Player.main.GetVehicle();
                var thisSeatruck = Player.main.GetComponentInParent<SeaTruckSegment>();

                if (__instance._constructed)
                {
                    return false;
                }
                int count = __instance.resourceMap.Count;
                int resourceID = __instance.GetResourceID();
                var backupConstructedAmount = __instance.constructedAmount;
                __instance.constructedAmount += Time.deltaTime / ((float)count * Constructable.GetConstructInterval());
                __instance.constructedAmount = Mathf.Clamp01(__instance.constructedAmount);
                int resourceID2 = __instance.GetResourceID();
                if (resourceID2 != resourceID)
                {
                    bool storageCheck = false;
                    TechType destroyTechType = __instance.resourceMap[resourceID2 - 1];
                    if (thisVehicle != null)
                    {
                        if (thisVehicle.GetType().Equals(typeof(Exosuit)))
                        {
                            StorageContainer storageContainer = ((Exosuit)thisVehicle).storageContainer;

                            if (storageContainer.container.Contains(destroyTechType) && GameModeUtils.RequiresIngredients())
                            {
                                storageContainer.container.DestroyItem(destroyTechType);
                                storageCheck = true;
                            }
                        }
                    }
                    else if(thisSeatruck != null)
                    {
                        SeaTruckSegment[] seaTruckSegments = thisSeatruck.GetComponents<SeaTruckSegment>();
                        List<StorageContainer[]> containers = new List<StorageContainer[]>();
                        foreach (SeaTruckSegment seaTruckSegment in seaTruckSegments)
                        {
                            containers.Add(seaTruckSegment.GetComponentsInChildren<StorageContainer>());
                        }
                        foreach (StorageContainer[] storageContainers in containers)
                        {
                            foreach (StorageContainer storageContainer in storageContainers)
                            {
                                if (storageContainer.container.Contains(destroyTechType) && GameModeUtils.RequiresIngredients())
                                {
                                    storageContainer.container.DestroyItem(destroyTechType);
                                    storageCheck = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!storageCheck)
                    {
                        __instance.constructedAmount = backupConstructedAmount;
                        return true;
                    }

                }
                __instance.UpdateMaterial();
                if (__instance.constructedAmount >= 1f)
                {
                    __instance.SetState(true, true);
                }
                ErrorMessage.AddMessage("in vehicle");
                return false;
            }
            else
            {
                ErrorMessage.AddMessage("not in vehicle");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Constructable))]
    [HarmonyPatch("Deconstruct")]
    internal class Deconstruct_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(Constructable __instance)
        {
            if (Player.main.GetVehicle() != null || Player.main.GetComponentInParent<SeaTruckSegment>() != null)
            {
                var thisVehicle = Player.main.GetVehicle();
                var thisSeatruck = Player.main.GetComponentInParent<SeaTruckSegment>();
                if (__instance._constructed)
                {
                    return true;
                }
                int count = __instance.resourceMap.Count;
                int resourceID = __instance.GetResourceID();
                var backupConstructedAmount = __instance.constructedAmount;
                __instance.constructedAmount -= Time.deltaTime / ((float)count * Constructable.GetConstructInterval());
                __instance.constructedAmount = Mathf.Clamp01(__instance.constructedAmount);
                int resourceID2 = __instance.GetResourceID();
                if (resourceID2 != resourceID && GameModeUtils.RequiresIngredients())
                {
                    bool storageCheck = false;
                    TechType techType = __instance.resourceMap[resourceID2];
                    GameObject gameObject = CraftData.InstantiateFromPrefab(techType, false);
                    Pickupable component = gameObject.GetComponent<Pickupable>();

                    if (thisVehicle != null)
                    {
                        if (thisVehicle.GetType().Equals(typeof(Exosuit)))
                        {
                            StorageContainer storageContainer = ((Exosuit)thisVehicle).storageContainer;

                            if (storageContainer.container.HasRoomFor(component) && GameModeUtils.RequiresIngredients())
                            {
                                var name = Language.main.Get(component.GetTechName());
                                ErrorMessage.AddMessage(Language.main.GetFormat("VehicleAddedToStorage", name));

                                uGUI_IconNotifier.main.Play(component.GetTechType(), uGUI_IconNotifier.AnimationType.From, null);

                                component.Initialize();

                                var item = new InventoryItem(component);
                                storageContainer.container.UnsafeAdd(item);
                                component.PlayPickupSound();
                                storageCheck = true;
                            }
                        }
                    }
                    else
                    {
                        SeaTruckSegment[] seaTruckSegments = thisSeatruck.GetComponents<SeaTruckSegment>();
                        List<StorageContainer[]> containers = new List<StorageContainer[]>();
                        foreach (SeaTruckSegment seaTruckSegment in seaTruckSegments)
                        {
                            containers.Add(seaTruckSegment.GetComponentsInChildren<StorageContainer>());
                        }
                        foreach (StorageContainer[] storageContainers in containers)
                        {
                            foreach (StorageContainer storageContainer in storageContainers)
                            {
                                if (storageContainer.container.HasRoomFor(component) && GameModeUtils.RequiresIngredients())
                                {
                                    var name = Language.main.Get(component.GetTechName());
                                    ErrorMessage.AddMessage(Language.main.GetFormat("VehicleAddedToStorage", name));

                                    uGUI_IconNotifier.main.Play(component.GetTechType(), uGUI_IconNotifier.AnimationType.From, null);

                                    component.Initialize();

                                    var item = new InventoryItem(component);
                                    storageContainer.container.UnsafeAdd(item);
                                    component.PlayPickupSound();
                                    storageCheck = true;
                                    break;
                                }
                            }
                        }
                        if (!storageCheck)
                        {
                            __instance.constructedAmount = backupConstructedAmount;
                            return true;
                        }
                    }
                }
                __instance.UpdateMaterial();
                if (__instance.constructedAmount <= 0f)
                {
                    return true;
                }

                ErrorMessage.AddMessage("in vehicle");
                return false;
            }
            else
            {
                ErrorMessage.AddMessage("not in vehicle");
                return true;
            }
        }
    }
}
