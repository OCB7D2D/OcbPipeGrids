namespace NodeFacilitator
{

    public class NetPkgWaterLevelQuery : NetPkgWorkerQuery<MsgFillStateQuery> { }
    public class NetPkgWaterLevelResponse : NetPkgWorkerAnswer<MsgFillStateResponse> { }

    public class NetPkgWaterExchangeQuery : NetPkgWorkerQuery<MsgWaterExchangeQuery> { }
    public class NetPkgWaterExchangeResponse : NetPkgWorkerAnswer<MsgWaterExchangeResponse> { }

    public class NetPkgActionAddVisualState : NetPkgWorkerQuery<ActionAddVisualState> { }
    public class NetPkgActionRemoveVisualState : NetPkgWorkerQuery<ActionRemoveVisualState> { }
    public class NetPkgUpdateVisualStates : NetPkgWorkerAnswer<MsgUpdateVisualStates> { }

}
