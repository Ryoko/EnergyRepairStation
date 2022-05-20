namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc.Base;

    public class ViewModelWindowEnergyRepairStation : BaseViewModel
    {
        public ViewModelWindowEnergyRepairStation(ProtoObjectEnergyRepairStation.PrivateState privateState)
        {
            PrivateState = privateState;

            ViewModelItemsContainerExchange = new ViewModelItemsContainerExchange(
                    privateState.ItemsContainer)
            {
                IsContainerTitleVisible = false,
                IsManagementButtonsVisible = false
            };

            ViewModelItemsContainerExchange.IsActive = true;
        }

        public ProtoObjectEnergyRepairStation.PrivateState PrivateState { get; }

        public ViewModelItemsContainerExchange ViewModelItemsContainerExchange { get; }
    }
}