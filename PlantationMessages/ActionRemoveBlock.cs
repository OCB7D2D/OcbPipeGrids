namespace NodeManager
{

    public class ActionRemoveFarmPlot : BaseActionRemoveBlock<NetPkgActionRemoveFarmPlot>
    {
        protected override void SetupNetPkg(NetPkgActionRemoveFarmPlot pkg) => pkg.Setup(this);
    }

    public class ActionRemovePlantGrowing : BaseActionRemoveBlock<NetPkgActionRemovePlantGrowing>
    {
        protected override void SetupNetPkg(NetPkgActionRemovePlantGrowing pkg) => pkg.Setup(this);
    }

    public class ActionRemoveComposter : BaseActionRemoveChest<NetPkgActionRemoveComposter>
    {
        protected override void SetupNetPkg(NetPkgActionRemoveComposter pkg) => pkg.Setup(this);
    }

}
