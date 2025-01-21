namespace Santa_Gifts_API.Core {
	/// <summary>
	/// A ranking of items.
	/// </summary>
	/// <typeparam name="T">The type of the items in the ranking.</typeparam>
	public class Ranking<T> {
		/// <summary>
		/// The number of items in the ranking.
		/// </summary>
		private static readonly List<KeyValuePair<double, T>> Heap = [];

		/// <summary>
		/// The set of items in the dropped ranking.
		/// </summary>
		private HashSet<T> _dropped = [];
		
		/// <summary>
		/// The set of items in the dropped ranking.
		/// </summary>
		public HashSet<T> Dropped => _dropped;

		/// <summary>
		/// The last index of the heap.
		/// </summary>
		private static int Last => Heap.Count - 1;

		/// <summary>
		/// Gets the number of items currently present in the ranking.
		/// </summary>
		public int Count => Heap.Count;

		/// <summary>
		/// Adds a new item to the ranking with the given rank.
		/// </summary>
		/// <param name="rank">The rank of the item to add.</param>
		/// <param name="item">The item to add.</param>
		public void In(T item, double rank) {
			Heap.Add(new(rank, item));
			HeapifyUp(Last);
		}

		/// <summary>
		/// Moves the item at the given index to the root of the heap by swapping it with its parent until the heap property is satisfied.
		/// </summary>
		/// <param name="index">The index of the item to move to the root of the heap.</param>
		private static void HeapifyUp(int index) {
			int parentIndex = (index - 1) / 2;

			if (index > 0 && Heap[index].Key < Heap[parentIndex].Key) {
				Swap(index, parentIndex);
				HeapifyUp(parentIndex);
			}
		}

		/// <summary>
		/// Retrieves the highest ranked item in the ranking and removes it from the ranking.
		/// </summary>
		/// <returns>The highest ranked item in the ranking.</returns>
		/// <exception cref="InvalidOperationException">Thrown when the rank is already empty.</exception>
		public T Out() {
			if (Count == 0) {
				throw new InvalidOperationException("The rank is already empty");
			}

			KeyValuePair<double, T> root = Heap[0];
			Heap[0] = Heap[Last];
			Heap.RemoveAt(Last);
			HeapifyDown(0);

			return root.Value;
		}

		/// <summary>
		/// Adds a new item to the ranking with the given rank and immediately retrieves the highest ranked item if the ranking exceeds the specified limit.
		/// </summary>
		/// <param name="rank">The rank of the item to add.</param>
		/// <param name="item">The item to add.</param>
		/// <param name="limit">The maximum number of items to keep in the ranking. Defaults to 0, which means the ranking has no limit.</param>
		/// <returns>The highest ranked item in the ranking if the ranking exceeds the specified limit; otherwise, the default value of type T.</returns>
		public T? InOut(T item, double rank, int limit=0) {
			In(item, rank);
			return Count > limit ? Out() : default;
		}

		/// <summary>
		/// Moves the item at the given index to the bottom of the heap by swapping it with its smallest child until the heap property is satisfied.
		/// </summary>
		/// <param name="index">The index of the item to move to the bottom of the heap.</param>
		private static void HeapifyDown(int index) {
			int left = 2 * index + 1;
			int right = 2 * index + 2;
			int smallest = GetSmallest(GetSmallest(index, left), right);

			if (smallest != index) {
				Swap(smallest, index);
				HeapifyDown(smallest);
			}
		}

		/// <summary>
		/// Returns the index of the smaller of the items at the given index and childIndex.
		/// If childIndex is out of range of the heap, the index is returned.
		/// </summary>
		/// <param name="index">The index of the item to compare.</param>
		/// <param name="childIndex">The index of the child item to compare.</param>
		/// <returns>The index of the smaller of the two items.</returns>
		private static int GetSmallest(int index, int childIndex) {
			if (childIndex > Last) return index;

			double parent = Heap[index].Key;
			double child = Heap[childIndex].Key;
			return child < parent ? childIndex : index;
		}
		
		/// <summary>
		/// Swaps the items at the specified indices in the heap.
		/// </summary>
		/// <param name="index">The index of the first item to swap.</param>
		/// <param name="parentIndex">The index of the second item to swap.</param>
		private static void Swap(int index, int parentIndex) {
			(Heap[parentIndex], Heap[index]) = (Heap[index], Heap[parentIndex]);
		}

		/// <summary>
		/// Empties the heap and returns all items it contains in a stack.
		/// Saves the dropped items in a HashSet.
		/// </summary>
		/// <returns>A stack of all items in the heap.</returns>
		public Stack<T> Drop() {
			Stack<T> items = new();
			_dropped = [];

			while (Count > 0) {
				T item = Out();
				items.Push(item);
				Dropped.Add(item);
			}
			
			return items;
		}
	}
}
