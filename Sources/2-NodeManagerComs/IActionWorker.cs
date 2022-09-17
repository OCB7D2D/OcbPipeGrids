namespace NodeFacilitator
{

    //########################################################
    // Action sent from anyone to worker
    // Mostly ever happens on server side
    // But remote clients my place queries
    //########################################################

    public interface IActionWorker
    {
        int SenderEntityId { get; set; }
        void ProcessOnWorker(NodeManagerWorker worker);
    }

    //########################################################
    //########################################################

}