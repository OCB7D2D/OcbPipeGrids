namespace NodeManager
{

    public class ActionAddPlantGrowing : BaseActionAddBlock<NetPkgActionAddPlantGrowing>
    {
        public override NodeBase CreatePipeNode() => new PlantationGrowing(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddPlantGrowing pkg) => pkg.Setup(this);
    }

}
