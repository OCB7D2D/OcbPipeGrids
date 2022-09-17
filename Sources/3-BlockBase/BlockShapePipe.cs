public class BlockShapePipe : BlockShapeNew
{

    public override void Init(Block _block)
    {
        base.Init(_block);
        //for (var i = 0; i < 6; i += 1)

    }

    public override bool isRenderFace(BlockValue bv, BlockFace face, BlockValue adj)
    {
        if (bv.ischild) return false;
        // Always render the model itself
        if ((int)face == 6) return true;
        // We only support simple rotations (90 only)
        // This avoids any issues when weird stuff is loaded
        // Which can happen if pipe block is placed by the game
        if (bv.rotation > 23 || adj.rotation > 23) return true;
        // Only hide face if both blocks are pipe connections
        if (!(bv.Block is IBlockConnection self)) return true;
        if (!(adj.Block is IBlockConnection other)) return true;
        // Check for matching connectors and same pipe diameter
        return self.PipeDiameter != other.PipeDiameter ||
            !self.CanConnect((byte)face, bv.rotation) ||
            !other.CanConnect(FullRotation.Mirror((byte)face), adj.rotation);
        // Otherwise render the lid to cover up the potential hole
    }

}
