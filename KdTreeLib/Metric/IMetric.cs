using System.Collections.Generic;

namespace KdTree3
{
	public interface IMetric
	{
		int DistanceSquared(Vector3i a, Vector3i b);
	}
}
