using System.Collections.Concurrent;
using System.IO;

namespace NodeManager
{

    public abstract class BlockRemotePowered : PoweredBase
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

        public override void CreateGridItem(Vector3i blockPos, BlockValue blockValue)
        {
            throw new System.NotImplementedException();
        }

        public override void RemoveGridItem(Vector3i blockPos)
        {
            throw new System.NotImplementedException();
        }

    }

}