namespace NodeManager
{

    public class NetPkgActionAddFarmLand : NetPkgWorkerAction<ActionAddFarmLand> { }
    public class NetPkgActionRemoveFarmLand : NetPkgWorkerAction<ActionRemoveFarmLand> { }

    public class NetPkgActionAddFarmPlot : NetPkgWorkerAction<ActionAddFarmPlot> { }
    public class NetPkgActionRemoveFarmPlot : NetPkgWorkerAction<ActionRemoveFarmPlot> { }

    public class NetPkgActionAddPlantGrowing : NetPkgWorkerAction<ActionAddPlantGrowing> { }
    public class NetPkgActionRemovePlantGrowing : NetPkgWorkerAction<ActionRemovePlantGrowing> { }

    public class NetPkgActionAddComposter : NetPkgWorkerAction<ActionAddComposter> { }
    public class NetPkgActionRemoveComposter : NetPkgWorkerAction<ActionRemoveComposter> { }

    public class NetPkgRemotePlantInteraction : NetPkgWorkerAction<MsgPlantInteraction> { }

}
