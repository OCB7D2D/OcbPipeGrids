namespace NodeManager
{

    public class ActionAddPlantGrowing : BaseActionAddBlock<NetPkgActionAddPlantGrowing>
    {
        public override NodeBase CreateNode() => new PlantationGrowing(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddPlantGrowing pkg) => pkg.Setup(this);
    }

}
