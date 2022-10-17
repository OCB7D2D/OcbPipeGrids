using System.Collections.Concurrent;
using System.IO;

namespace NodeManager
{

	public abstract class BlockRemote : BlockBase
	{

		public override string GetCustomDescription(
			Vector3i pos, BlockValue bv)
		{
			return NodeManagerInterface.Instance.Mother
				.GetCustomDescription(pos, bv);
		}

		public override string GetActivationText(
			WorldBase world, BlockValue bv,
			int clrIdx, Vector3i pos,
			EntityAlive focused)
		{
			return base.GetActivationText(world, bv, clrIdx, pos, focused)
				+ "\n" + GetCustomDescription(pos, bv);
		}

		//########################################################
		// Implementation for Grid Manager
		//########################################################
		public override void CreateGridItem(Vector3i position, BlockValue bv)
		{
			var action = new ActionAddBlock();
			action.Setup(position, bv);
			action.SetStorageID(TYPES.Sprinkler);
			NodeManagerInterface.SendToServer(action);
		}

		public override void RemoveGridItem(Vector3i position)
		{
			var action = new ActionRemoveBlock();
			action.Setup(position);
			NodeManagerInterface.SendToServer(action);
		}

	}

}