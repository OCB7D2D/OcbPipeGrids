namespace NodeManager
{

    public class ActionRemovePlantGrowing : BaseActionRemoveBlock<NetPkgActionRemovePlantGrowing>
    {
        protected override void SetupNetPkg(NetPkgActionRemovePlantGrowing pkg) => pkg.Setup(this);
    }

}
