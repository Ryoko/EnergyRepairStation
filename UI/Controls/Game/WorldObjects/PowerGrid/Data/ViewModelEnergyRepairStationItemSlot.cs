namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;

    public class ViewModelEnergyRepairStationItemSlot : BaseViewModel
    {
        private uint? durabilityValueCurrent;

        private IItem item;

        private static readonly Brush BrushGreen
            = Api.Client.UI.GetApplicationResource<Brush>("BrushColorGreen4");

        private static readonly Brush BrushRed
            = Api.Client.UI.GetApplicationResource<Brush>("BrushColorRed5");

        private static readonly Brush BrushYellow
            = Api.Client.UI.GetApplicationResource<Brush>("BrushColor5");

        public Brush BarBrush { get; private set; }

        public uint DurabilityValueCurrent
        {
            get => durabilityValueCurrent ?? 0;
            private set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (durabilityValueCurrent == value)
                {
                    return;
                }

                durabilityValueCurrent = value;
                BarBrush = GetBarBrush();
                NotifyThisPropertyChanged();
            }
        }

        public uint DurabilityValueMax { get; private set; }

        public IItem Item
        {
            get => item;
            set
            {
                if (item == value)
                {
                    return;
                }

                ReleaseSubscriptions();
                item = value;
                NotifyThisPropertyChanged();

                durabilityValueCurrent = null;

                if (item is not null)
                {
                    if (TrySetupDurabilityItem())
                    {
                        return;
                    }
                }

                // the item is null or doesn't provide charge info
                DurabilityValueMax = 1;
                DurabilityValueCurrent = 0;
                BarBrush = Brushes.Transparent;
            }
        }

        private Brush GetBarBrush()
        {
            var fraction = this.DurabilityValueCurrent / (double)this.DurabilityValueMax;
            if (fraction >= ItemDurabilitySystem.ThresholdFractionGreenStatus)
            {
                return BrushGreen;
            }

            if (fraction >= ItemDurabilitySystem.ThresholdFractionYellowStatus)
            {
                return BrushYellow;
            }

            return BrushRed;
        }

        private bool TrySetupDurabilityItem()
        {
            if (item.ProtoItem is not IProtoItemWithDurability protoItemWithDurability)
            { 
                return false; 
            }

            var capacity = protoItemWithDurability.DurabilityMax;
            if (capacity <= 0)
            {
                return false;
            }

            DurabilityValueMax = (uint)capacity;

            var privateState = item.GetPrivateState<ItemWithDurabilityPrivateState>();
            privateState.ClientSubscribe(_ => _.DurabilityCurrent,
                                         value => DurabilityValueCurrent = (uint)value,
                                         this);

            DurabilityValueCurrent = (uint)privateState.DurabilityCurrent;
            return true;
        }
    }
}