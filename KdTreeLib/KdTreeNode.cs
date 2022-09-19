using System;
using System.Text;

namespace KdTree3
{
	public partial class KdTree<TMetric>
		where TMetric : IMetric
	{

		public partial class Vector3i<TValue>
		{
			[Serializable]
			public class Node
			{
				public Node(Vector3i point, TValue value)
				{
					Point = point;
					Value = value;
				}

				public Vector3i Point;
				public TValue Value = default;

				internal Node LeftChild = null;
				internal Node RightChild = null;

				internal ref Node this[int compare]
				{
					get
					{
						if (compare <= 0)
							return ref LeftChild;
						else
							return ref RightChild;
					}
				}

				public bool IsLeaf
				{
					get
					{
						return (LeftChild == null) && (RightChild == null);
					}
				}

				public override string ToString()
				{
					var sb = new StringBuilder();

					sb.Append(Point.ToString() + "\t");

					if (Value == null)
						sb.Append("null");
					else
						sb.Append(Value.ToString());

					return sb.ToString();
				}
			}
		}

	}
}