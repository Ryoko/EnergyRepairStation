namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc.Base;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;

    public class ObjectEnergyRepairStation : ProtoObjectEnergyRepairStation
    {
        public override byte ContainerInputSlotsCount => 3;

        public override string Description =>
            "Used to repair broken equipment using energy.";

        public override bool IsRelocatable => true;

        public override string Name => "Energy repair station";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override double RepairPerSecondPerSlot => 4;

        public override float StructurePointsMax => 2000;

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryElectricity>();

            build.StagesCount = 5;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemComponentsElectronic>(count: 4);
            build.AddStageRequiredItem<ItemComponentsHighTech>(count: 4);
            build.AddStageRequiredItem<ItemOrePragmium>(count: 4);
            build.AddStageRequiredItem<ItemIngotSteel>(count: 2);

            repair.StagesCount = 5;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemComponentsElectronic>(count: 2);
            repair.AddStageRequiredItem<ItemComponentsHighTech>(count: 2);
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.9, 0.6), offset: (0.05, 0))
                .AddShapeRectangle(size: (0.8, 0.4), offset: (0.1, 0.6), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.9, 1.17), offset: (0.05, 0), group: CollisionGroups.ClickArea);
        }
    }
}