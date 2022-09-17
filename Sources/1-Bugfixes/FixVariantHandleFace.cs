using HarmonyLib;

namespace NodeFacilitator
{

    //########################################################
    // Patch to fix issue with HandleFace and Variant Helper
    // Issue shows if you have a variant helper where some
    // Blocks don't have the same HandleFace setting. The
    // variant helper will wrongly use its own HandleFace.
    //########################################################

    [HarmonyPatch(typeof(BlockPlacement))]
    [HarmonyPatch("OnPlaceBlock")]
    public class OnPlaceBlock
    {

        // Overwrite HandleFace of the VariantHelper Block
        // Use HandleFace of selected alternate/variant block
        // Store previous setting just to play nice (not needed)
        static void Prefix(BlockValue _bv, out BlockFace __state)
        {
            __state = _bv.Block.HandleFace;
            if (!_bv.Block.SelectAlternates) return;
            EntityPlayerLocal player = GameManager.Instance.World.GetPrimaryPlayer();
            BlockValue alt = _bv.Block.GetAltBlockValue(player.inventory.holdingItemItemValue.Meta);
            if (alt.type != BlockValue.Air.type) _bv.Block.HandleFace = alt.Block.HandleFace;
        }

        // Reset HandleFace of VariantHelper block again
        static void Postfix(BlockValue _bv, BlockFace __state)
        {
            if (!_bv.Block.SelectAlternates) return;
            _bv.Block.HandleFace = __state;
        }

    }

    //########################################################
    //########################################################

}
