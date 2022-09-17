using System;

namespace PipeManager
{
    public class PipeGridClient
    {

        Vector3i RequestedCustomDesc = Vector3i.invalid;
        Vector3i AcquiredCustomDesc = Vector3i.invalid;
        string CustomDescription = String.Empty;

        internal string GetCustomDescription(Vector3i position, BlockValue bv)
        {
            // Log.Out("Get custom desc {0} vs {1}", position, AcquiredCustomDesc);
            if (AcquiredCustomDesc == position)
            {
                return CustomDescription;
            }
            else if (RequestedCustomDesc != position)
            {
                var query = new MsgDescriptionQuery();
                query.Setup(position);
                RequestedCustomDesc = position;
                PipeGridInterface.SendToServer(query);

            }
            return "Waiting for server";
        }

        internal void OnDescriptionResponse(MsgDescriptionResponse msg)
        {
            Log.Out("Description came back => {0} -> {1}",
                msg.Position, msg.Description);
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
                RequestedCanConnect = position;
                Log.Out("Query CanConnect at {0}", query.Position);
                PipeGridInterface.SendToServer(query);
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
