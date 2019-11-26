using System;
using System.Collections.Generic;
using SMLHelper.V2.Crafting;

namespace BuilderModule
{
    internal class SeatruckBuilderModulePrefab : Craftable
    {
        public static TechType TechTypeID { get; private set; }

        internal SeatruckBuilderModulePrefab()
            : base(nameID: "SeaTruckBuilderModule",
                  nameUsingForFiles: "BuilderModule",
                  friendlyName: "Sea Truck Builder Module",
                  description: "Allows you to build bases while in a SeaTruck.",
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
                new Ingredient(TechType.Builder, 1),
                new Ingredient(TechType.AdvancedWiringKit, 1)
            };

            RecipeData Props = new RecipeData();
            Props.craftAmount = 1;
            Props.Ingredients = ingredients;


            return Props;
        }
    }
}
