using UnityEngine;
using UWE;
using System.Text;
using System.Collections.Generic;
using System;

namespace ToggleModule
{
    public class VehicleToggleModule : MonoBehaviour
    {
        public VehicleToggleModule Instance { get; private set; }
        public int moduleSlotID { get; set; }
        private Vehicle thisVehicle { get; set; }
        private Player playerMain { get; set; }
        private EnergyMixin energyMixin { get; set; }

        private bool isToggle;
        private bool isActive;

        private int handleInputFrame = -1;
        private string noPowerText;




        public void Awake()
        {
            Instance = this;
            thisVehicle = Instance.GetComponent<Vehicle>();
            energyMixin = thisVehicle.GetComponent<EnergyMixin>();
            playerMain = Player.main;
        }


        private void Start()
        {
            thisVehicle.onToggle += this.OnToggle;
            thisVehicle.modules.onAddItem += this.OnAddItem;
            thisVehicle.modules.onRemoveItem += this.OnRemoveItem;
        }

        private void OnRemoveItem(InventoryItem item)
        {
            if (item.item.GetTechType() == VehicleToggleModulePrefab.TechTypeID)
            {                
                this.moduleSlotID = -1;
                Instance.enabled = false;
                OnDisable();
            }
        }

        private void OnAddItem(InventoryItem item)
        {
            if (item.item.GetTechType() == VehicleToggleModulePrefab.TechTypeID)
            {
                this.moduleSlotID = thisVehicle.GetSlotByItem(item);
                ErrorMessage.AddMessage("VehicleToggleModule installed in Slot: " + (this.moduleSlotID-1));
                Instance.enabled = true;
            }
        }

        private void OnToggle(int slotID, bool state)
        {
            if (thisVehicle.GetSlotBinding(slotID) == VehicleToggleModulePrefab.TechTypeID)
            {
                this.isToggle = state;

                if (this.isToggle)
                {
                    ErrorMessage.AddMessage("Toggle Module Enabled");
                    OnEnable();
                }
                else
                {
                    ErrorMessage.AddMessage("Toggle Module Disabled");
                    OnDisable();
                }
            }
        }

        public void OnEnable()
        {
            this.isActive = playerMain.isPiloting && this.isToggle && this.moduleSlotID > -1;
        }

        public void OnDisable()
        {
            this.isActive = false;
        }


        private void Update()
        {
            if (this.isActive)
            {
                this.UpdateText();
                this.HandleInput();
            }
        }
        private void UpdateText()
        {
            this.noPowerText = Language.main.Get("NoPower");
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
            bool buttonHeld = GameInput.GetButtonHeld(GameInput.Button.AltTool);
            bool buttonDown = GameInput.GetButtonDown(GameInput.Button.Deconstruct);
            bool buttonHeld2 = GameInput.GetButtonHeld(GameInput.Button.Deconstruct);
            bool quicker = GameInput.GetButtonHeld(GameInput.Button.Sprint);
        }

        private bool TryDisplayNoPowerTooltip()
        {
            if (this.energyMixin.charge <= 0f)
            {
                HandReticle main = HandReticle.main;
                main.SetText(HandReticle.TextType.Hand ,this.noPowerText, false);
                main.SetIcon(HandReticle.IconType.Default, 1f);
                return true;
            }
            return false;
        }
        
        private void OnDestroy()
        {
            OnDisable();
            thisVehicle.onToggle -= OnToggle;
            thisVehicle.modules.onAddItem -= OnAddItem;
            thisVehicle.modules.onRemoveItem -= OnRemoveItem;
        } 
    }
}

