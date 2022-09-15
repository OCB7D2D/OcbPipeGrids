namespace PipeManager
{

    public class NetPkgDescriptionQuery : NetPkgWorkerQuery<MsgDescriptionQuery> { }
    public class NetPkgDescriptionResponse : NetPkgWorkerAnswer<MsgDescriptionResponse> { }

    public class NetPkgConnectorQuery : NetPkgWorkerQuery<MsgConnectorQuery> { }
    public class NetPkgConnectorResponse : NetPkgWorkerAnswer<MsgConnectorResponse> { }
    public class NetPkgActionAddConnection : NetPkgWorkerQuery<ActionAddConnection> { }
    public class NetPkgActionRemoveConnection : NetPkgWorkerQuery<ActionRemoveConnection> { }


    public class NetPkgActionAddIrrigation : NetPkgWorkerQuery<ActionAddIrrigation> { }
    public class NetPkgActionRemoveIrrigation : NetPkgWorkerQuery<ActionRemoveIrrigation> { }

}
