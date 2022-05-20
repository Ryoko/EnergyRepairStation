namespace AtomicTorch.CBND.CoreMod.Scripts.Technologies.Tier4.Industry
{
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Industry;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;

    public class TechNodeEnergyRepairStation : TechNode<TechGroupIndustryT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectEnergyRepairStation>();
        }
    }
}