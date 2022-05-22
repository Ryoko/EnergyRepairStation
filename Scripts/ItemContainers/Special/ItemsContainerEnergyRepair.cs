namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemsContainerEnergyRepair : ProtoItemsContainer
    {
        public override bool CanAddItem(CanAddItemContext context)
        {
            //return context.Item.ProtoItem is IProtoItemWithDurability;
            var item = context.Item;
            if (item.ProtoItem
                    is not IProtoItemWithDurability { DurabilityMax: > 0, IsRepairable: true } )
            {
                // not a repairable item
                return false;
            }

            return true;
        }
    }
}