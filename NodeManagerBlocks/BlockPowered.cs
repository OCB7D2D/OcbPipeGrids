using System.Collections.Concurrent;
using System.IO;

namespace NodeManager
{

    public abstract class PoweredBase : BlockPowered, IBlockNode
    {

		Block IBlockNode.BLK => this;

        public abstract TYPES NodeType { get; }

        public override void OnBlockAdded(
			WorldBase world, Chunk chunk,
			Vector3i pos, BlockValue bv)
		{
			base.OnBlockAdded(world, chunk, pos, bv);
			if (!NodeManagerInterface.HasServer) return;
			PipeBlockHelper.OnBlockAdded(this, pos, bv);
		}

		public override void OnBlockRemoved(
			WorldBase world, Chunk chunk,
			Vector3i pos, BlockValue bv)
		{
			base.OnBlockRemoved(world, chunk, pos, bv);
			if (!NodeManagerInterface.HasServer) return;
			PipeBlockHelper.OnBlockRemoved(this, pos, bv);
		}

		public abstract void CreateGridItem(Vector3i blockPos, BlockValue blockValue);

		public abstract void RemoveGridItem(Vector3i blockPos);

	}

}