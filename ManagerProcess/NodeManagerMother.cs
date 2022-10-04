using System;
using System.Collections.Concurrent;

namespace NodeManager
{
    public class NodeManagerMother
    {
        public readonly ConcurrentQueue<IActionWorker> ToWorker;
        public readonly ConcurrentQueue<IActionClient> ToMother;

        Vector3i RequestedCustomDesc = Vector3i.invalid;
        Vector3i AcquiredCustomDesc = Vector3i.invalid;
        string CustomDescription = String.Empty;
        ulong RenewCustomDesc = 0;

        public NodeManagerMother(
            ConcurrentQueue<IActionWorker> input,
            ConcurrentQueue<IActionClient> output)
        {
            ToWorker = input;
            ToMother = output;
        }

        internal string GetCustomDescription(Vector3i position, BlockValue bv)
        {
            // Log.Out("Get custom desc {0} vs {1}", position, AcquiredCustomDesc);
            if (AcquiredCustomDesc == position)
            {
                if (RenewCustomDesc < GameTimer.Instance.ticks)
                {
                    RequireCustomDesc(position);
                }
                return CustomDescription;
            }
            else if (RequestedCustomDesc != position)
            {
                RequireCustomDesc(position);
            }
            return "Waiting for server";
        }

        private void RequireCustomDesc(Vector3i position)
        {
            var query = new MsgDescriptionQuery();
            query.Setup(position);
            RequestedCustomDesc = position;
            NodeManagerInterface.SendToServer(query);
            RenewCustomDesc = GameTimer.Instance.ticks + 15;
        }

        internal void OnDescriptionResponse(MsgDescriptionResponse msg)
        {
            AcquiredCustomDesc = msg.Position;
            CustomDescription = msg.Description;
        }

        Vector3i RequestedCanConnect = Vector3i.invalid;
        MsgConnectorResponse AcquiredCanConnect = null;

        public bool HasPendingCanConnect(Vector3i position)
        {
            return AcquiredCanConnect?.Position != position;
        }

        public void OnConnectResponse(MsgConnectorResponse msg)
        {
            if (RequestedCanConnect == msg.Position)
                AcquiredCanConnect = msg;
        }

        public bool CanConnect(Vector3i position, BlockConnector connector)
        {
            if (position == AcquiredCanConnect?.Position)
            {
                return ConnectionHelper.CanAddConnector(
                    connector, AcquiredCanConnect.NB);
            }
            else if (RequestedCanConnect != position)
            {
                // Cancel old request, do a new one
                // var query = new NetPkgConnectorQuery();

                // GameManager.Instance.WorldInfo

                var query = new MsgConnectorQuery();
                query.Setup(position); // Two steps
                if (connector.GetBlock(out IBlockConnection blk))
                {
                    Log.Out("Set query diameter");
                    query.SetPipeDiameter(blk.PipeDiameter);
                }
                RequestedCanConnect = position;
                Log.Out("Query CanConnect at {0}", query.Position);
                NodeManagerInterface.SendToServer(query);
                return false;
            }
            else
            {
                // Log.Out("Waiting for response");
                return false;
            }
        }

    }
}
