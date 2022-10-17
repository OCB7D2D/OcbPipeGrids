namespace NodeManager
{

    public class ActionAddFarmLand : ActionAddMyBlock<NetPkgActionAddFarmLand>
    {
        public override NodeBase CreateNode() => new PlantationFarmLand(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddFarmLand pkg) => pkg.Setup(this);
    }

    public class ActionAddFarmPlot : ActionAddMyBlock<NetPkgActionAddFarmPlot>
    {
        public override NodeBase CreateNode() => new PlantationFarmPlot(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddFarmPlot pkg) => pkg.Setup(this);
    }

    public class ActionAddPlantGrowing : ActionAddMyBlock<NetPkgActionAddPlantGrowing>
    {
        public override NodeBase CreateNode() => new PlantationGrowing(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddPlantGrowing pkg) => pkg.Setup(this);
    }

    public class ActionAddSprinkler : ActionAddMyBlock<NetPkgActionAddSprinkler>
    {
        public override NodeBase CreateNode() => new PlantationSprinkler(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddSprinkler pkg) => pkg.Setup(this);
    }

    public class ActionAddComposter : BaseActionAddChest<NetPkgActionAddComposter>
    {
        public override NodeBase CreateNode() => new PlantationComposter(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddComposter pkg) => pkg.Setup(this);
    }


}
