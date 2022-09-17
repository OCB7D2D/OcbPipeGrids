using UnityEngine;

namespace NodeFacilitator
{
    public partial class NodeManagerClient
    {

        Vector3i RequestedCanConnect = Vector3i.invalid;
        MsgConnectorResponse AcquiredCanConnect = null;

        public bool HasPendingCanConnect(Vector3i position)
        {
            return AcquiredCanConnect?.Position != position
                && RequestedCustomDesc == position;
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
                    connector, AcquiredCanConnect.NB,
                    Input.GetKey(KeyCode.LeftShift));
            }
            else if (RequestedCanConnect != position)
            {
                var query = new MsgConnectorQuery();
                query.Setup(position);
                RequestedCanConnect = position;
                NodeManagerInterface.SendToWorker(query);
                return false;
            }
            else
            {
                return false;
            }
        }

    }
}
