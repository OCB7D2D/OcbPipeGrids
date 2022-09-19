using System;
using System.Collections.Generic;
using System.Linq;

namespace KdTree3
{
	public partial class NearestNeighbourList<TItem>
	{
		private const int DefaultCapacity = 32;

		public interface INearestNeighbourList
		{
			bool Add(TItem item, int distance);
			int FurtherestDistance { get; }
			bool IsFull { get; }
		}

		public class UnlimitedList : INearestNeighbourList
		{
			List<Tuple<TItem, int>> _items;

			public UnlimitedList() : this(DefaultCapacity) { }
			public UnlimitedList(int capacity) => _items = new List<Tuple<TItem, int>>(capacity);

			public int FurtherestDistance => default;

			public bool IsFull => false;

			public int Count => _items.Count;

			public bool Add(TItem item, int distance)
			{
				_items.Add(new Tuple<TItem, int>(item, distance));
				return true;
			}

			public TItem[] GetSortedArray() => _items.OrderBy(x => x.Item2).Select(x => x.Item1).ToArray();

			public void Clear() => _items.Clear();
		}

		public class List : INearestNeighbourList
		{
			public List(int maxCount, int capacity)
			{
				MaxCount = maxCount;
				queue = new PriorityQueue<TItem>(capacity);
			}

			public List(int maxCount) : this(maxCount, DefaultCapacity) { }
			public List() : this(int.MaxValue, DefaultCapacity) { }

			private PriorityQueue<TItem> queue;
			public int MaxCount { get; }

			public int Count { get { return queue.Count; } }

			public bool Add(TItem item, int distance)
			{
				if (queue.Count >= MaxCount)
				{
					// If the distance of this item is less than the distance of the last item
					// in our neighbour list then pop that neighbour off and push this one on
					// otherwise don't even bother adding this item
					if (distance.CompareTo(queue.GetHighestPriority()) < 0)
					{
						queue.Dequeue();
						queue.Enqueue(item, distance);
						return true;
					}
					else
						return false;
				}
				else
				{
					queue.Enqueue(item, distance);
					return true;
				}
			}

			public int FurtherestDistance => queue.GetHighestPriority();
			public bool IsFull => Count == MaxCount;

			public TItem RemoveFurtherest()
			{
				return queue.Dequeue();
			}

			public TItem[] GetSortedArray()
			{
				var count = Count;
				var neighbourArray = new TItem[count];

				for (var index = 0; index < count; index++)
				{
					var n = RemoveFurtherest();
					neighbourArray[count - index - 1] = n;
				}

				return neighbourArray;
			}
		}
	}
}