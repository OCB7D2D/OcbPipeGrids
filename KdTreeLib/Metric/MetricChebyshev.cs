using System;
using System.Runtime.CompilerServices;

namespace KdTree3
{
	public struct MetricChebyshev : IMetric
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int DistanceSquared(Vector3i a, Vector3i b)
		{
			var dx = a.x - b.x;
			var dy = a.y - b.y;
			var dz = a.z - b.z;
			return Math.Max(dx * dx,
				Math.Max(dy * dy, dz * dz));
		}
	}
}
