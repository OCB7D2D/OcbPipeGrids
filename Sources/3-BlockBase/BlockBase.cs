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

	}

}