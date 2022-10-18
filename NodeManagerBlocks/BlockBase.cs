using System.Collections.Concurrent;
using System.IO;

namespace NodeManager
{

    public abstract class BlockBase : Block, IBlockNode
    {
        // public override void Init()
        // {
        //     base.Init();
        // }

        Block IBlockNode.BLK => this;
		// public abstract ushort StorageID { get; }
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

		// public abstract void CreateGridItem(Vector3i blockPos, BlockValue blockValue);

		// public abstract void RemoveGridItem(Vector3i blockPos);

	}

}