namespace KdTree3
{

	public partial class KdTree<TMetric>
		where TMetric : IMetric
	{
		public struct HyperRect
		{
			public Vector3i MinPoint;
			public Vector3i MaxPoint;

			public HyperRect(Vector3i minPoint, Vector3i maxPoint)
			{
				MinPoint = minPoint;
				MaxPoint = maxPoint;
			}

			static HyperRect()
			{
				Infinite = new HyperRect()
				{
					MinPoint = new Vector3i(int.MinValue, int.MinValue, int.MinValue),
					MaxPoint = new Vector3i(int.MaxValue, int.MaxValue, int.MaxValue)
				};
			}

			public static readonly HyperRect Infinite;

			public void GetClosestPoint(Vector3i to, ref Vector3i result)
			{
				result.x = MinPoint.x > to.x ? MinPoint.x :
					MaxPoint.x < to.x ? MaxPoint.x : to.x;
				result.y = MinPoint.y > to.y ? MinPoint.y :
					MaxPoint.y < to.y ? MaxPoint.y : to.y;
				result.z = MinPoint.z > to.z ? MinPoint.z :
					MaxPoint.z < to.z ? MaxPoint.z : to.z;
			}
		}

	}

}
