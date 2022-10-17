namespace NodeManager
{

    public class NetPkgDescriptionQuery : NetPkgWorkerAction<MsgDescriptionQuery> { }
    public class NetPkgDescriptionResponse : NetPkgWorkerAnswer<MsgDescriptionResponse> { }

    public class NetPkgConnectorQuery : NetPkgWorkerAction<MsgConnectorQuery> { }
    public class NetPkgConnectorResponse : NetPkgWorkerAnswer<MsgConnectorResponse> { }

    public class NetPkgWaterLevelQuery : NetPkgWorkerAction<MsgWaterLevelQuery> { }
    public class NetPkgWaterLevelResponse : NetPkgWorkerAnswer<MsgWaterLevelResponse> { }

    public class NetPkgWaterExchangeQuery : NetPkgWorkerAction<MsgWaterExchangeQuery> { }
    public class NetPkgWaterExchangeResponse : NetPkgWorkerAnswer<MsgWaterExchangeResponse> { }

    public class NetPkgActionAddBlock : NetPkgWorkerAction<ActionAddBlock> { }
    public class NetPkgActionRemoveBlock : NetPkgWorkerAction<ActionRemoveBlock> { }

}
