using System;

namespace KdTree3
{
	public struct MetricManhatten : IMetric
	{
		public int DistanceSquared(Vector3i a, Vector3i b)
		{
			int dx = Math.Abs(a.x - b.x);
			int dy = Math.Abs(a.y - b.y);
			int dz = Math.Abs(a.z - b.z);
			int m = dx + dy + dz;
			return m * m;
		}
	}
}
