using System;
using System.Collections.Generic;
using SMLHelper.V2.Crafting;

namespace ToggleModule
{
    internal class VehicleToggleModulePrefab : Craftable
    {
        public static TechType TechTypeID { get; private set; }

        internal VehicleToggleModulePrefab()
            : base(nameID: "VehicleToggleModule",
                  nameUsingForFiles: "ToggleModule",
                  friendlyName: "Vehicle Toggle Module",
                  description: "Basic code to add a toggleable module to a Vehicle type (Prawnsuit, Seamoth, Hoverbike).",
                  template: TechType.SeamothSonarModule,
                  fabricatorType: CraftTree.Type.SeamothUpgrades,
                  fabricatorTab: "ExosuitModules",
                  requiredAnalysis: TechType.BaseUpgradeConsole,
                  groupForPDA: TechGroup.VehicleUpgrades,
                  categoryForPDA: TechCategory.VehicleUpgrades,
                  equipmentType: EquipmentType.VehicleModule,
                  quickSlotType: QuickSlotType.Toggleable,
                  itemSize: new Vector2int(1, 1),
                  gamerResourceFileName: null
                  )
        {
        }

        public override void Patch()
        {
            base.Patch();
            TechTypeID = TechType;
        }

        protected override RecipeData GetRecipe()
        {
            List<Ingredient> ingredients = new List<Ingredient>() {
                new Ingredient(TechType.Titanium, 1),
                new Ingredient(TechType.Quartz, 1)
            };

            RecipeData Props = new RecipeData();
            Props.craftAmount = 1;
            Props.Ingredients = ingredients;


            return Props;
        }
    }
}
