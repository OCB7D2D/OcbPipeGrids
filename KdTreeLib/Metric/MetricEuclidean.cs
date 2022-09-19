namespace KdTree3
{
	public struct MetricEuclidean : IMetric
	{
		public int DistanceSquared(Vector3i a, Vector3i b)
		{
			int dx = a.x - b.x;
			int dy = a.y - b.y;
			int dz = a.z - b.z;
			return dx * dx + dy * dy + dz * dz;
		}
	}
}
