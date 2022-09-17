namespace NodeFacilitator
{

    //########################################################
    // Action sent from worker to mother
    // Only ever happens on server side
    //########################################################

    public interface IActionMother
    {
        void ProcessAtMother(NodeManagerInterface api);
    }

    //########################################################
    //########################################################

}