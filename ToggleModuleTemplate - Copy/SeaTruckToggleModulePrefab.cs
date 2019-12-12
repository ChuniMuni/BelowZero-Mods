using System;
using System.Collections.Generic;
using SMLHelper.V2.Crafting;

namespace ToggleModule
{
    internal class SeaTruckToggleModulePrefab : Craftable
    {
        public static TechType TechTypeID { get; private set; }

        internal SeaTruckToggleModulePrefab()
            : base(nameID: "SeaTruckToggleModule",
                  nameUsingForFiles: "ToggleModule",
                  friendlyName: "Sea Truck Toggle Module",
                  description: "Base code for a toggleable SeaTruck module.",
                  template: TechType.SeaTruckUpgradeAfterburner,
                  fabricatorType: CraftTree.Type.SeamothUpgrades,
                  fabricatorTab: "SeaTruckUpgrade",
                  requiredAnalysis: TechType.BaseUpgradeConsole,
                  groupForPDA: TechGroup.VehicleUpgrades,
                  categoryForPDA: TechCategory.VehicleUpgrades,
                  equipmentType: EquipmentType.SeaTruckModule,
                  quickSlotType: QuickSlotType.Passive,
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
