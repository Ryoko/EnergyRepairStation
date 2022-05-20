namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc.Base
{
    using System;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public abstract class ProtoObjectEnergyRepairStation
        : ProtoObjectStructure
          <ProtoObjectEnergyRepairStation.PrivateState,
              StaticObjectElectricityConsumerPublicState,
              StaticObjectClientState>,
          IInteractableProtoWorldObject,
          IProtoObjectElectricityConsumerWithCustomRate
    {
        public abstract byte ContainerInputSlotsCount { get; }

        public virtual ElectricityThresholdsPreset DefaultConsumerElectricityThresholds
            => new(startupPercent: 20,
                   shutdownPercent: 10);

        public double ElectricityConsumptionPerSecondWhenActive { get; private set; }

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public override bool IsRelocatable => true;

        /// <summary>
        /// EnergyRepair speed in EU/s (per slot).
        /// </summary>
        public abstract double RepairPerSecondPerSlot { get; }

        public override double ServerUpdateIntervalSeconds => 5;

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);

            var privateState = GetPrivateState(gameObject);
            ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
                gameObject.OccupiedTile,
                privateState.ItemsContainer);
        }

        public virtual double SharedGetCurrentElectricityConsumptionRate(IStaticWorldObject worldObject)
        {
            var count = 0;
            var items = GetPrivateState(worldObject).ItemsContainer.Items;
            foreach (var item in items)
            {
                if (SharedCanRepairItemWithDurability(item))
                {
                    count++;
                }
            }

            var consumptionRate = count / (double)ContainerInputSlotsCount;
            return consumptionRate;
        }

        BaseUserControlWithWindow IInteractableProtoWorldObject.ClientOpenUI(IWorldObject worldObject)
        {
            var staticWorldObject = (IStaticWorldObject)worldObject;
            var privateState = GetPrivateState(staticWorldObject);
            return ClientOpenUI(staticWorldObject, privateState);
        }

        void IInteractableProtoWorldObject.ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
        {
        }

        void IInteractableProtoWorldObject.ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
        {
        }

        IObjectElectricityStructurePrivateState IProtoObjectElectricityConsumer.GetPrivateState(
            IStaticWorldObject worldObject)
        {
            return GetPrivateState(worldObject);
        }

        IObjectElectricityConsumerPublicState IProtoObjectElectricityConsumer.GetPublicState(
            IStaticWorldObject worldObject)
        {
            return GetPublicState(worldObject);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            PowerGridSystem.ClientInitializeConsumerOrProducer(data.GameObject);
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected virtual BaseUserControlWithWindow ClientOpenUI(
            IStaticWorldObject worldObject,
            PrivateState privateState)
        {
            return WindowEnergyRepairStation.Open(
                new ViewModelWindowEnergyRepairStation(privateState));
        }

        protected virtual void PrepareProtoObjectEnergyRepairStation()
        {
        }

        protected sealed override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();

            ElectricityConsumptionPerSecondWhenActive =
                RepairPerSecondPerSlot * ContainerInputSlotsCount;

            PrepareProtoObjectEnergyRepairStation();
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var privateState = data.PrivateState;

            // setup input container to allow only power banks on input
            var itemsContainer = privateState.ItemsContainer;
            var itemsSlotsCount = ContainerInputSlotsCount;
            if (itemsContainer is not null)
            {
                // container already created - update slots count
                Server.Items.SetSlotsCount(itemsContainer, slotsCount: itemsSlotsCount);
                return;
            }

            itemsContainer = Server.Items.CreateContainer<ItemsContainerEnergyRepair>(
                owner: data.GameObject,
                slotsCount: itemsSlotsCount);

            privateState.ItemsContainer = itemsContainer;
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            if (data.PublicState.ElectricityConsumerState != ElectricityConsumerState.PowerOnActive)
            {
                return;
            }

            var items = GetPrivateState(data.GameObject).ItemsContainer.Items;
            var deltaTime = data.DeltaTime;
            foreach (var item in items)
            {
                ServerTryRepairItemWithDurability(item, deltaTime);
            }
        }

        private static bool SharedCanRepairItemWithDurability(IItem item)
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

            var itemPrivateState = item.GetPrivateState<ItemWithDurabilityPrivateState>();
            var durability = itemPrivateState.DurabilityCurrent;
            return durability < capacity;
        }

        private void ServerTryRepairItemWithDurability(IItem item, double deltaTime)
        {
            if (item.ProtoItem is not IProtoItemWithDurability protoItemWithDurability)
            {
                return;
            }

            var durabilityMax = protoItemWithDurability.DurabilityMax;
            if (durabilityMax <= 0)
            {
                return;
            }

            var privateState = item.GetPrivateState<ItemWithDurabilityPrivateState>();
            var durability = privateState.DurabilityCurrent;
            if (durability >= durabilityMax)
            {
                return;
            }

            // recharge this item
            var deltaRepair = deltaTime * RepairPerSecondPerSlot;
            durability += (uint)deltaRepair;
            durability = Math.Min(durability, durabilityMax);
            privateState.DurabilityCurrent = durability;
        }

        public class PrivateState : StructurePrivateState, IObjectElectricityStructurePrivateState
        {
            [SyncToClient]
            public ElectricityThresholdsPreset ElectricityThresholds { get; set; }

            [SyncToClient]
            public IItemsContainer ItemsContainer { get; set; }

            [SyncToClient]
            [TempOnly]
            public byte PowerGridChargePercent { get; set; }
        }
    }
}