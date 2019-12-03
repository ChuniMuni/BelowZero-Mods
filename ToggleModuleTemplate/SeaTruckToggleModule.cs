using UnityEngine;
using UWE;
using System.Text;
using System.Collections.Generic;
using System;

namespace ToggleModule
{
    public class SeaTruckToggleModule : MonoBehaviour
    {
        public SeaTruckToggleModule Instance { get; private set; }
        private SeaTruckUpgrades thisVehicle { get; set; }
        private Player playerMain { get; set; }
        private PowerRelay energyMixin { get; set; }

        private bool isToggle;
        private bool isActive;
        int lastToggled = 0;
        public int moduleSlotID = -1;
        private int handleInputFrame = -1;
        private string noPowerText;




        public void Awake()
        {
            Instance = this;
            thisVehicle = Instance.GetComponent<SeaTruckUpgrades>();
            energyMixin = thisVehicle.relay;
            playerMain = Player.main;
        }


        private void Start()
        {
            thisVehicle.modules.onEquip += OnAddItem;
            thisVehicle.modules.onUnequip += OnRemoveItem;
        }

        private void OnRemoveItem(string slot, InventoryItem item)
        {
            if (CraftData.GetTechType(item.item.gameObject) == SeaTruckToggleModulePrefab.TechTypeID)
            {
                Instance.enabled = false;
                moduleSlotID = -1;
                OnDisable();
            }
        }

        private void OnAddItem(string slot, InventoryItem item)
        {
            if (thisVehicle.modules.GetTechTypeInSlot(slot) == SeaTruckToggleModulePrefab.TechTypeID)
            {
                int.TryParse(slot.Replace("SeaTruckModule", ""), out moduleSlotID);
                ErrorMessage.AddMessage("Toggle Module installed in Slot: " + moduleSlotID);
                Instance.enabled = true;
                OnEnable();
            }
        }

        public bool OnEnable()
        {
            isToggle = true;
            isActive = isToggle && thisVehicle.modules.GetCount(SeaTruckToggleModulePrefab.TechTypeID) > 0;
            if (isActive)
            {
                ErrorMessage.AddMessage("Toggle Module Enabled");
                lastToggled = 30;
                return isActive;
            }
            else
            {
                isToggle = false;
                return isActive;
            }
        }

        public void OnDisable()
        {
            ErrorMessage.AddMessage("Toggle Module Disabled");
            isToggle = false;
            isActive = false;
            lastToggled = 30;
        }


        private void Update()
        {
            if (playerMain.GetComponentInParent<SeaTruckUpgrades>() != thisVehicle && this.isToggle)
            {
                OnDisable();
            }
            lastToggled--;
            if (isActive)
            {
                this.UpdateText();
                this.HandleInput();
            }
            if (moduleSlotID > -1)
            {
                if (lastToggled <= 0 && Player.main.GetQuickSlotKeyHeld(moduleSlotID - 1) && playerMain.GetComponentInParent<SeaTruckUpgrades>() == thisVehicle)
                {
                    if (isToggle)
                    {
                        OnDisable();
                    }
                    else
                    {
                        OnEnable();
                    }
                }
            }
        }

        private void HandleInput()
        {
            if (this.handleInputFrame == Time.frameCount)
            {
                return;
            }
            this.handleInputFrame = Time.frameCount;
            if (!AvatarInputHandler.main.IsEnabled())
            {
                return;
            }
            if (this.TryDisplayNoPowerTooltip())
            {
                return;
            }
            Targeting.AddToIgnoreList(Player.main.gameObject);
            Targeting.AddToIgnoreList(thisVehicle.gameObject);
            GameObject gameObject;
            float num;
            Targeting.GetTarget(null, 60f, out gameObject, out num);
            if (gameObject == null)
            {
                return;
            }
            bool buttonHeld = GameInput.GetButtonHeld(GameInput.Button.LeftHand);
            bool buttonDown = GameInput.GetButtonDown(GameInput.Button.RightHand);
            bool buttonHeld2 = GameInput.GetButtonHeld(GameInput.Button.RightHand);
            bool quickbuild = GameInput.GetButtonHeld(GameInput.Button.Sprint);
        }

        private bool TryDisplayNoPowerTooltip()
        {
            if (this.energyMixin.GetPower() <= 0f)
            {
                HandReticle main = HandReticle.main;
                main.SetText(HandReticle.TextType.Hand, this.noPowerText, false);
                main.SetIcon(HandReticle.IconType.Default, 1f);
                return true;
            }
            return false;
        }

        private void UpdateText()
        {
            this.noPowerText = Language.main.Get("NoPower");
        }

        private void OnDestroy()
        {
            OnDisable();
            thisVehicle.modules.onEquip -= OnAddItem;
            thisVehicle.modules.onUnequip -= OnRemoveItem;
        }
    }
}

