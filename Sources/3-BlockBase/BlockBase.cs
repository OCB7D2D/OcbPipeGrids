using System.Collections.Concurrent;
using System.IO;

namespace NodeFacilitator
{

    public abstract class BlockBase : Block, IBlockNode
    {

        Block IBlockNode.BLK => this;

		public abstract TYPES NodeType { get; }

		public override void OnBlockAdded(
			WorldBase world, Chunk chunk,
			Vector3i pos, BlockValue bv)
		{
			base.OnBlockAdded(world, chunk, pos, bv);
			if (!NodeManagerInterface.HasServer) return;
			NodeBlockHelper.OnBlockAdded(this, pos, bv);
		}

		public override void OnBlockRemoved(
			WorldBase world, Chunk chunk,
			Vector3i pos, BlockValue bv)
		{
			base.OnBlockRemoved(world, chunk, pos, bv);
			if (!NodeManagerInterface.HasServer) return;
			NodeBlockHelper.OnBlockRemoved(this, pos, bv);
		}

        public override void OnBlockValueChanged(
			WorldBase world, Chunk chunk, int clrIdx,
			Vector3i pos, BlockValue old_bv, BlockValue new_bv)
        {
            base.OnBlockValueChanged(world, chunk,
				clrIdx, pos, old_bv, new_bv);
            if (!NodeManagerInterface.HasServer) return;
            NodeBlockHelper.OnBlockValueChanged(this, pos, new_bv);
        }

    }

}