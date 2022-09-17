using System;

namespace NodeFacilitator
{
    public partial class NodeManagerClient
    {

        Vector3i RequestedCustomDesc = Vector3i.invalid;
        Vector3i AcquiredCustomDesc = Vector3i.invalid;
        string CustomDescription = String.Empty;
        ulong RenewCustomDesc = 0;

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
            NodeManagerInterface.SendToWorker(query);
            RenewCustomDesc = GameTimer.Instance.ticks + 15;
        }

        internal void OnDescriptionResponse(MsgDescriptionResponse msg)
        {
            AcquiredCustomDesc = msg.Position;
            CustomDescription = msg.Description;
        }

    }
}
