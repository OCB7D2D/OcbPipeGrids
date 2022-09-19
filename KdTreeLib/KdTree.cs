using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace KdTree3
{
	public partial class KdTree<TMetric>
		where TMetric : IMetric
	{

		static readonly TMetric metric = default;

		[Serializable]
		public partial class Vector3i<TValue> : IEnumerable<Tuple<Vector3i, TValue>>
		{
			// public TMetric Metric;

			public Vector3i()
			{
				Count = 0;
			}

			public Vector3i(AddDuplicateBehavior addDuplicateBehavior)
			{
				AddDuplicateBehavior = addDuplicateBehavior;
			}

			private Node root = null;

			public AddDuplicateBehavior AddDuplicateBehavior { get; private set; }

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private int Increment(int value)
			{
				value++;
				if (value >= 3) return 0;
				return value;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int CompareVectorDimension(Vector3i point, Vector3i parent, int dimension)
			{
				if (dimension == 0) return point.x.CompareTo(parent.x);
				else if (dimension == 1) return point.y.CompareTo(parent.y);
				else if (dimension == 2) return point.z.CompareTo(parent.z);
				else throw new Exception("Invalid Dimension for Point");
				// int compare = TypeMath.Compare(point[dimension], parent.Point[dimension]);
			}

			public bool Add(Vector3i point, TValue value)
			{
				var nodeToAdd = new Node(point, value);

				if (root == null)
				{
					root = new Node(point, value);
				}
				else
				{
					int dimension = -1;
					Node parent = root;

					do
					{
						// Increment the dimension we're searching in
						dimension = Increment(dimension);

						// Does the node we're adding have the same hyperpoint as this node?
						if (point == parent.Point)
						{
							switch (AddDuplicateBehavior)
							{
								case AddDuplicateBehavior.Skip:
									return false;

								case AddDuplicateBehavior.Error:
									throw new DuplicateNodeError();

								case AddDuplicateBehavior.Update:
									parent.Value = value;
									return true;

								default:
									// Should never happen
									throw new Exception("Unexpected AddDuplicateBehavior");
							}
						}

						// Which side does this node sit under in relation to it's parent at this level?
						int compare = CompareVectorDimension(point, parent.Point, dimension);

						if (parent[compare] == null)
						{
							parent[compare] = nodeToAdd;
							break;
						}
						else
						{
							parent = parent[compare];
						}
					}
					while (true);
				}

				Count++;
				return true;
			}

			private void ReaddChildNodes(Node removedNode)
			{
				if (removedNode.IsLeaf)
					return;

				// The folllowing code might seem a little redundant but we're using 
				// 2 queues so we can add the child nodes back in, in (more or less) 
				// the same order they were added in the first place
				var nodesToReadd = new Queue<Node>();

				var nodesToReaddQueue = new Queue<Node>();

				if (removedNode.LeftChild != null)
					nodesToReaddQueue.Enqueue(removedNode.LeftChild);

				if (removedNode.RightChild != null)
					nodesToReaddQueue.Enqueue(removedNode.RightChild);

				while (nodesToReaddQueue.Count > 0)
				{
					var nodeToReadd = nodesToReaddQueue.Dequeue();

					nodesToReadd.Enqueue(nodeToReadd);

					for (int side = -1; side <= 1; side += 2)
					{
						if (nodeToReadd[side] != null)
						{
							nodesToReaddQueue.Enqueue(nodeToReadd[side]);

							nodeToReadd[side] = null;
						}
					}
				}

				while (nodesToReadd.Count > 0)
				{
					var nodeToReadd = nodesToReadd.Dequeue();

					Count--;
					Add(nodeToReadd.Point, nodeToReadd.Value);
				}
			}

			public void RemoveAt(Vector3i point)
			{
				// Is tree empty?
				if (root == null)
					return;

				Node node;

				if (point == root.Point)
				{
					node = root;
					root = null;
					Count--;
					ReaddChildNodes(node);
					return;
				}

				node = root;

				int dimension = -1;
				do
				{
					dimension = Increment(dimension);

					int compare = CompareVectorDimension(point, node.Point, dimension);

					if (node[compare] == null)
						// Can't find node
						return;

					if (point == node[compare].Point)
					{
						var nodeToRemove = node[compare];
						node[compare] = null;
						Count--;

						ReaddChildNodes(nodeToRemove);
					}
					else
						node = node[compare];
				}
				while (node != null);
			}

			public void GetNearestNeighbours(Vector3i point, NearestNeighbourList<Tuple<Vector3i, TValue>>.INearestNeighbourList results)
			{
				var rect = HyperRect.Infinite;
				AddNearestNeighbours(root, point, rect, 0, results, int.MaxValue);
			}

			public Tuple<Vector3i, TValue>[] GetNearestNeighbours(Vector3i point, int count = int.MaxValue)
			{
				if (count > Count)
					count = Count;

				if (count < 0)
				{
					throw new ArgumentException("Number of neighbors cannot be negative");
				}

				if (count == 0)
					return new Tuple<Vector3i, TValue>[0];

				var nearestNeighbours = CreateNearestNeighbourList(count);

				var rect = HyperRect.Infinite;

				AddNearestNeighbours(root, point, rect, 0, nearestNeighbours, int.MaxValue);

				count = nearestNeighbours.Count;

				return nearestNeighbours.GetSortedArray();
			}

			/*
			 * 1. Search for the target
			 * 
			 *   1.1 Start by splitting the specified hyper rect
			 *       on the specified node's point along the current
			 *       dimension so that we end up with 2 sub hyper rects
			 *       (current dimension = depth % dimensions)
			 *   
			 *	 1.2 Check what sub rectangle the the target point resides in
			 *	     under the current dimension
			 *	     
			 *   1.3 Set that rect to the nearer rect and also the corresponding 
			 *       child node to the nearest rect and node and the other rect 
			 *       and child node to the further rect and child node (for use later)
			 *       
			 *   1.4 Travel into the nearer rect and node by calling function
			 *       recursively with nearer rect and node and incrementing 
			 *       the depth
			 * 
			 * 2. Add leaf to list of nearest neighbours
			 * 
			 * 3. Walk back up tree and at each level:
			 * 
			 *    3.1 Add node to nearest neighbours if
			 *        we haven't filled our nearest neighbour
			 *        list yet or if it has a distance to target less
			 *        than any of the distances in our current nearest 
			 *        neighbours.
			 *        
			 *    3.2 If there is any point in the further rectangle that is closer to
			 *        the target than our furtherest nearest neighbour then travel into
			 *        that rect and node
			 * 
			 *  That's it, when it finally finishes traversing the branches 
			 *  it needs to we'll have our list!
			 */

			private void AddNearestNeighbours(
				Node node,
				Vector3i target,
				HyperRect rect,
				int depth,
				NearestNeighbourList<Tuple<Vector3i, TValue>>.INearestNeighbourList nearestNeighbours,
				int maxSearchRadiusSquared)
			{

				if (node == null) return;

				// Split our hyper-rect into 2 sub rects along the
				// current node's point on the current dimension
				// Note: this makes a copy of the structs
				HyperRect leftRect = rect;
				HyperRect rightRect = rect;
				bool goLeft = false;

				if (depth == 0)
				{
					leftRect.MaxPoint.x = node.Point.x;
					rightRect.MinPoint.x = node.Point.x;
					goLeft = target.x <= node.Point.x;
				}
				else if (depth == 1)
				{
					leftRect.MaxPoint.y = node.Point.y;
					rightRect.MinPoint.y = node.Point.y;
					goLeft = target.y <= node.Point.y;
				}
				else if (depth == 2)
				{
					leftRect.MaxPoint.z = node.Point.z;
					rightRect.MinPoint.z = node.Point.z;
					goLeft = target.z <= node.Point.z;
				}

				HyperRect nearerRect = goLeft ? leftRect : rightRect;
				HyperRect furtherRect = goLeft ? rightRect : leftRect;

				Node nearerNode = goLeft ? node.LeftChild : node.RightChild;
				Node furtherNode = goLeft ? node.RightChild : node.LeftChild;

				// Let's walk down into the nearer branch
				if (nearerNode != null)
				{
					AddNearestNeighbours(
						nearerNode,
						target,
						nearerRect,
						Increment(depth),
						nearestNeighbours,
						maxSearchRadiusSquared);
				}

				// Walk down into the further branch but only if our capacity hasn't been reached 
				// OR if there's a region in the further rect that's closer to the target than our
				// current furtherest nearest neighbour
				Vector3i closestPointInFurtherRect = Vector3i.zero;
				furtherRect.GetClosestPoint(target, ref closestPointInFurtherRect);
				var distanceSquaredToTarget = metric.DistanceSquared(closestPointInFurtherRect, target);

				if (distanceSquaredToTarget <= maxSearchRadiusSquared)
				{
					if (!nearestNeighbours.IsFull || distanceSquaredToTarget < nearestNeighbours.FurtherestDistance)
					{
						AddNearestNeighbours(
							furtherNode,
							target,
							furtherRect,
							Increment(depth),
							nearestNeighbours,
							maxSearchRadiusSquared);
					}
				}

				// Try to add the current node to our nearest neighbours list
				distanceSquaredToTarget = metric.DistanceSquared(node.Point, target);

				if (distanceSquaredToTarget <= maxSearchRadiusSquared)
					nearestNeighbours.Add(new Tuple<Vector3i, TValue>(node.Point, node.Value), distanceSquaredToTarget);

			}

			/// <summary>
			/// Performs a radial search up to a maximum count.
			/// </summary>
			/// <param name="center">Center point</param>
			/// <param name="radius">Radius to find neighbours within</param>
			/// <param name="count">Maximum number of neighbours</param>
			public Tuple<Vector3i, TValue>[] RadialSearch(Vector3i center, int radius, int maxCapacity = int.MaxValue)
			{
				var results = CreateNearestNeighbourList(maxCapacity);
				RadialSearch(center, radius, results);
				return results.GetSortedArray();
			}

			public void RadialSearch(Vector3i center, int radius, NearestNeighbourList<Tuple<Vector3i, TValue>>.INearestNeighbourList results)
			{
				AddNearestNeighbours(
					root,
					center,
					HyperRect.Infinite,
					0,
					results,
					radius * radius);
			}

			public int Count { get; private set; }

			public bool ContainsKey(Vector3i point)
			{
				var parent = root;
				int dimension = -1;
				do
				{
					if (parent == null)
					{
						return false;
					}
					else if (point == parent.Point)
					{
						return true;
					}

					// Keep searching
					dimension = Increment(dimension);
					int compare = CompareVectorDimension(point, parent.Point, dimension);
					parent = parent[compare];
				}
				while (true);
			}

			public bool TryFindValueAt(Vector3i point, out TValue value)
			{
				var parent = root;
				int dimension = -1;
				do
				{
					if (parent == null)
					{
						value = default;
						return false;
					}
					else if (point == parent.Point)
					{
						value = parent.Value;
						return true;
					}

					// Keep searching
					dimension = Increment(dimension);
					int compare = CompareVectorDimension(point, parent.Point, dimension);
					parent = parent[compare];
				}
				while (true);
			}

			public TValue FindValueAt(Vector3i point)
			{
				if (TryFindValueAt(point, out TValue value))
					return value;
				else
					return default;
			}

			public bool TryFindValue(TValue value, out Vector3i point)
			{
				if (root == null)
				{
					point = default;
					return false;
				}

				// First-in, First-out list of nodes to search
				var nodesToSearch = new Queue<Node>();

				nodesToSearch.Enqueue(root);

				while (nodesToSearch.Count > 0)
				{
					var nodeToSearch = nodesToSearch.Dequeue();

					if (nodeToSearch.Value.Equals(value))
					{
						point = nodeToSearch.Point;
						return true;
					}
					else
					{
						for (int side = -1; side <= 1; side += 2)
						{
							var childNode = nodeToSearch[side];

							if (childNode != null)
								nodesToSearch.Enqueue(childNode);
						}
					}
				}

				point = default;
				return false;
			}

			public Vector3i FindValue(TValue value)
			{
				if (TryFindValue(value, out Vector3i point))
					return point;
				else
					return default;
			}

			private void AddNodeToStringBuilder(Node node, StringBuilder sb, int depth)
			{
				sb.AppendLine(node.ToString());

				for (var side = -1; side <= 1; side += 2)
				{
					for (var index = 0; index <= depth; index++)
						sb.Append("\t");

					sb.Append(side == -1 ? "L " : "R ");

					if (node[side] == null)
						sb.AppendLine("");
					else
						AddNodeToStringBuilder(node[side], sb, depth + 1);
				}
			}

			public override string ToString()
			{
				if (root == null)
					return "";

				var sb = new StringBuilder();
				AddNodeToStringBuilder(root, sb, 0);
				return sb.ToString();
			}

			private void AddNodesToList(Node node, List<Node> nodes)
			{
				if (node == null)
					return;

				nodes.Add(node);

				for (var side = -1; side <= 1; side += 2)
				{
					if (node[side] != null)
					{
						AddNodesToList(node[side], nodes);
						node[side] = null;
					}
				}
			}

			private void SortNodesArray(Node[] nodes, int byDimension, int fromIndex, int toIndex)
			{
				for (var index = fromIndex + 1; index <= toIndex; index++)
				{
					var newIndex = index;

					while (true)
					{
						var a = nodes[newIndex - 1];
						var b = nodes[newIndex];
						if (CompareVectorDimension(b.Point, a.Point, byDimension) < 0)
						{
							nodes[newIndex - 1] = b;
							nodes[newIndex] = a;
						}
						else
							break;
					}
				}
			}

			private void AddNodesBalanced(Node[] nodes, int byDimension, int fromIndex, int toIndex)
			{
				if (fromIndex == toIndex)
				{
					Add(nodes[fromIndex].Point, nodes[fromIndex].Value);
					nodes[fromIndex] = null;
					return;
				}

				// Sort the array from the fromIndex to the toIndex
				SortNodesArray(nodes, byDimension, fromIndex, toIndex);

				// Find the splitting point
				int midIndex = fromIndex + (int)System.Math.Round((toIndex + 1 - fromIndex) / 2f) - 1;

				// Add the splitting point
				Add(nodes[midIndex].Point, nodes[midIndex].Value);
				nodes[midIndex] = null;

				// Recurse
				int nextDimension = Increment(byDimension);

				if (fromIndex < midIndex)
					AddNodesBalanced(nodes, nextDimension, fromIndex, midIndex - 1);

				if (toIndex > midIndex)
					AddNodesBalanced(nodes, nextDimension, midIndex + 1, toIndex);
			}

			public void Balance()
			{
				var nodeList = new List<Node>();
				AddNodesToList(root, nodeList);

				Clear();

				AddNodesBalanced(nodeList.ToArray(), 0, 0, nodeList.Count - 1);
			}

			private void RemoveChildNodes(Node node)
			{
				for (var side = -1; side <= 1; side += 2)
				{
					if (node[side] != null)
					{
						RemoveChildNodes(node[side]);
						node[side] = null;
					}
				}
			}

			public void Clear()
			{
				if (root != null)
					RemoveChildNodes(root);
			}

			public IEnumerator<Tuple<Vector3i, TValue>> GetEnumerator()
			{
				var left = new Stack<Node>();
				var right = new Stack<Node>();

				void addLeft(Node node)
				{
					if (node.LeftChild != null)
					{
						left.Push(node.LeftChild);
					}
				}

				void addRight(Node node)
				{
					if (node.RightChild != null)
					{
						right.Push(node.RightChild);
					}
				}

				if (root != null)
				{
					yield return new Tuple<Vector3i, TValue>(root.Point, root.Value);

					addLeft(root);
					addRight(root);

					while (true)
					{
						if (left.Any())
						{
							var item = left.Pop();

							addLeft(item);
							addRight(item);

							yield return new Tuple<Vector3i, TValue>(item.Point, item.Value);
						}
						else if (right.Any())
						{
							var item = right.Pop();

							addLeft(item);
							addRight(item);

							yield return new Tuple<Vector3i, TValue>(item.Point, item.Value);
						}
						else
						{
							break;
						}
					}
				}
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			public static NearestNeighbourList<Tuple<Vector3i, TValue>>.List CreateNearestNeighbourList()
				=> new NearestNeighbourList<Tuple<Vector3i, TValue>>.List();

			public static NearestNeighbourList<Tuple<Vector3i, TValue>>.List CreateNearestNeighbourList(int maxCount)
				=> new NearestNeighbourList<Tuple<Vector3i, TValue>>.List(maxCount);

			public static NearestNeighbourList<Tuple<Vector3i, TValue>>.List CreateNearestNeighbourList(int maxCount, int capacity)
				=> new NearestNeighbourList<Tuple<Vector3i, TValue>>.List(maxCount, capacity);

			public static NearestNeighbourList<Tuple<Vector3i, TValue>>.UnlimitedList CreateUnlimitedList()
				=> new NearestNeighbourList<Tuple<Vector3i, TValue>>.UnlimitedList();

			public static NearestNeighbourList<Tuple<Vector3i, TValue>>.UnlimitedList CreateUnlimitedList(int capacity)
				=> new NearestNeighbourList<Tuple<Vector3i, TValue>>.UnlimitedList(capacity);
		}

	}
}
