﻿using UnityEngine;
using UWE;
using System.Text;
using System.Collections.Generic;
using System;

namespace BuilderModule
{
    public class BuilderModule : MonoBehaviour
    {
        public BuilderModule Instance { get; private set; }
        public int moduleSlotID { get; set; }
        private Vehicle thisVehicle { get; set; }
        private Player playerMain { get; set; }
        private EnergyMixin energyMixin { get; set; }

        private bool isToggle;
        private bool isActive;

        public float powerConsumptionConstruct = 0.5f;
        public float powerConsumptionDeconstruct = 0.5f;
        public FMOD_CustomLoopingEmitter buildSound;
        private FMODAsset completeSound;
        private Constructable constructable;
        private int handleInputFrame = -1;
        private string deconstructText;
        private string constructText;
        private string noPowerText;




        public void Awake()
        {
            Instance = this;
            thisVehicle = Instance.GetComponent<Vehicle>();
            energyMixin = thisVehicle.GetComponent<EnergyMixin>();
            playerMain = Player.main;
            var builderPrefab = Resources.Load<GameObject>("WorldEntities/Tools/Builder").GetComponent<BuilderTool>();
            completeSound = Instantiate(builderPrefab.completeSound, gameObject.transform);
        }


        private void Start()
        {
            thisVehicle.onToggle += OnToggle;
            thisVehicle.modules.onAddItem += OnAddItem;
            thisVehicle.modules.onRemoveItem += OnRemoveItem;
        }

        private void OnRemoveItem(InventoryItem item)
        {
            if (item.item.GetTechType() == BuilderModulePrefab.TechTypeID)
            {                
                moduleSlotID = -1;
                Instance.enabled = false;
                OnDisable();
            }
        }

        private void OnAddItem(InventoryItem item)
        {
            if (item.item.GetTechType() == BuilderModulePrefab.TechTypeID)
            {
                moduleSlotID = thisVehicle.GetSlotByItem(item);
                ErrorMessage.AddMessage("Builder Module installed in Slot: " + moduleSlotID);
                Instance.enabled = true;
                OnEnable();
            }
        }

        private void OnToggle(int slotID, bool state)
        {
            if (thisVehicle.GetSlotBinding(slotID) == BuilderModulePrefab.TechTypeID)
            {
                isToggle = state;

                if (isToggle)
                {
                    ErrorMessage.AddMessage("Builder Module Enabled");
                    OnEnable();
                }
                else
                {
                    ErrorMessage.AddMessage("Builder Module Disabled");
                    OnDisable();
                }
            }
        }

        public void OnEnable()
        {
            isActive = playerMain.isPiloting && isToggle && moduleSlotID > -1;
        }

        public void OnDisable()
        {
            isActive = false;
            uGUI_BuilderMenu.Hide();
            Builder.End();
        }


        private void Update()
        {
            this.UpdateText();
            if (isActive)
            {
                if (thisVehicle.GetActiveSlotID() != moduleSlotID)
                {
                    thisVehicle.SlotKeyDown(thisVehicle.GetActiveSlotID());
                    thisVehicle.SlotKeyUp(thisVehicle.GetActiveSlotID());
                }
                if (GameInput.GetButtonDown(GameInput.Button.PDA) && !Player.main.GetPDA().isOpen && !Builder.isPlacing)
                {
                    if (energyMixin.charge > 0f)
                    {
                        Player.main.GetPDA().Close();
                        uGUI_BuilderMenu.Show();
                        handleInputFrame = Time.frameCount;
                    }
                }
                if (Builder.isPlacing)
                {
                    if (Player.main.GetLeftHandDown())
                    {
                        UWE.Utils.lockCursor = true;
                    }
                    if (UWE.Utils.lockCursor && GameInput.GetButtonDown(GameInput.Button.AltTool))
                    {
                        if (Builder.TryPlace())
                        {
                            Builder.End();
                        }
                    }
                    else if (this.handleInputFrame != Time.frameCount && GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                    {   
                        Builder.End();
                    }
                    FPSInputModule.current.EscapeMenu();
                    Builder.Update();
                }
                if (!uGUI_BuilderMenu.IsOpen() && !Builder.isPlacing)
                {
                    this.HandleInput();
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
            bool flag = this.TryDisplayNoPowerTooltip();
            if (flag)
            {
                return;
            }
            Targeting.AddToIgnoreList(Player.main.gameObject);
            GameObject gameObject;
            float num;
            Targeting.GetTarget(null, 60f, out gameObject, out num);
            if (gameObject == null)
            {
                return;
            }
            bool buttonHeld = GameInput.GetButtonHeld(GameInput.Button.AltTool);
            bool buttonDown = GameInput.GetButtonDown(GameInput.Button.Deconstruct);
            bool buttonHeld2 = GameInput.GetButtonHeld(GameInput.Button.Deconstruct);
            Constructable constructable = gameObject.GetComponentInParent<Constructable>();
            if (constructable != null && num > constructable.placeMaxDistance*4)
            {
                constructable = null;
            }
            if (constructable != null)
            {
                this.OnHover(constructable);
                string text;
                if (buttonHeld)
                {
                    this.Construct(constructable, true);
                    this.Construct(constructable, true);
                    this.Construct(constructable, true);
                    this.Construct(constructable, true);
                }
                else if (constructable.DeconstructionAllowed(out text))
                {
                    if (buttonHeld2)
                    {
                        if (constructable.constructed)
                        {
                            constructable.SetState(false, false);
                        }
                        else
                        {
                            this.Construct(constructable, false);
                            this.Construct(constructable, false);
                            this.Construct(constructable, false);
                            this.Construct(constructable, false);
                        }
                    }
                }
                else if (buttonDown && !string.IsNullOrEmpty(text))
                {
                    ErrorMessage.AddMessage(text);
                }
            }
            else
            {
                BaseDeconstructable baseDeconstructable = gameObject.GetComponentInParent<BaseDeconstructable>();
                if (baseDeconstructable == null)
                {
                    BaseExplicitFace componentInParent = gameObject.GetComponentInParent<BaseExplicitFace>();
                    if (componentInParent != null)
                    {
                        baseDeconstructable = componentInParent.parent;
                    }
                }
                else
                {
                    string text;
                    if (baseDeconstructable.DeconstructionAllowed(out text))
                    {
                        this.OnHover(baseDeconstructable);
                        if (buttonDown)
                        {
                            baseDeconstructable.Deconstruct();
                        }
                    }
                    else if (buttonDown && !string.IsNullOrEmpty(text))
                    {
                        ErrorMessage.AddMessage(text);
                    }
                }
            }
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

        private void UpdateText()
        {
            string buttonFormat = LanguageCache.GetButtonFormat("ConstructFormat", GameInput.Button.LeftHand);
            string buttonFormat2 = LanguageCache.GetButtonFormat("DeconstructFormat", GameInput.Button.Deconstruct);
            this.constructText = Language.main.GetFormat<string, string>("ConstructDeconstructFormat", buttonFormat, buttonFormat2);
            this.deconstructText = buttonFormat2;
            this.noPowerText = Language.main.Get("NoPower");
        }
        private bool Construct(Constructable c, bool state)
        {
            if (c != null && !c.constructed && this.energyMixin.charge > 0f)
            {
                float amount = ((!state) ? this.powerConsumptionDeconstruct : this.powerConsumptionConstruct) * Time.deltaTime;
                this.energyMixin.ConsumeEnergy(amount);
                bool constructed = c.constructed;
                bool flag = (!state) ? c.Deconstruct() : c.Construct();
                if (flag)
                {
                    this.constructable = c;
                }
                else if (state && !constructed)
                {
                    global::Utils.PlayFMODAsset(this.completeSound, c.transform, 20f);
                }
                return true;
            }
            return false;
        }

        private void OnHover(Constructable constructable)
        {
            if (isActive)
            {
                HandReticle main = HandReticle.main;
                if (constructable.constructed)
                {
                    main.SetText(HandReticle.TextType.Hand, this.deconstructText, false);
                }
                else
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine(this.constructText);
                    foreach (KeyValuePair<TechType, int> keyValuePair in constructable.GetRemainingResources())
                    {
                        TechType key = keyValuePair.Key;
                        string text = Language.main.Get(key);
                        int value = keyValuePair.Value;
                        if (value > 1)
                        {
                            stringBuilder.AppendLine(Language.main.GetFormat<string, int>("RequireMultipleFormat", text, value));
                        }
                        else
                        {
                            stringBuilder.AppendLine(text);
                        }
                    }
                    main.SetText(HandReticle.TextType.Hand, stringBuilder.ToString(), false);
                    main.SetProgress(constructable.amount);
                    main.SetIcon(HandReticle.IconType.Progress, 1.5f);
                }
            }
        }

        private void OnHover(BaseDeconstructable deconstructable)
        {
            if (isActive)
            {
                HandReticle main = HandReticle.main;
                main.SetText(HandReticle.TextType.Hand, this.deconstructText, false);
            }
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

