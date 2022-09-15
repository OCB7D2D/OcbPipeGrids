using System;

namespace KdTree.Math
{
	[Serializable]
	public class IntCubeMath : IntegerMath
	{
		public override int DistanceSquaredBetweenPoints(int[] a, int[] b)
		{
			int distance = Zero;
			int dimensions = a.Length;
			for (var dimension = 0; dimension < dimensions; dimension++)
			{
				int distOnThisAxis = Subtract(a[dimension], b[dimension]);
				int distOnThisAxisSquared = Multiply(distOnThisAxis, distOnThisAxis);
				distance = System.Math.Max(distance, distOnThisAxisSquared);
			}
			return distance;
		}
	}
}