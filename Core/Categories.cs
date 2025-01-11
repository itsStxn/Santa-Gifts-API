namespace Santa_Gifts_API.Core {
	/// <summary>
	/// The Categories class is responsible for managing the dictionary of word
	/// categories and their respective counts in the dataset.
	/// </summary>
	public class Categories {
		/// <summary>
		/// The dictionary of word categories and their respective counts.
		/// </summary>
		/// <remarks>
		/// The dictionary is used to store the counts of each category
		/// in the dataset.
		/// </remarks>
		private readonly Dictionary<string, int> Stats = [];
		
		/// <summary>
		/// The total count of all categories in the dataset.
		/// </summary>
		private int Count { get; set; }

		/// <summary>
		/// Constructs a new instance of the Categories class.
		/// </summary>
		/// <param name="category">The first category to add to the dataset.</param>
		/// <remarks>
		/// The constructor provides a convenient method to create a new instance
		/// of the Categories class by initializing the dictionary with the first
		/// category.
		/// </remarks>
		public Categories(string category) {
			Add(category);
		}

		/// <summary>
		/// Adds the specified category to the dataset, updating its count.
		/// </summary>
		/// <param name="category">The category to add.</param>
		/// <remarks>
		/// If the category already exists in the dataset, its count is incremented.
		/// Otherwise, the category is added to the dataset with an initial count of 1.
		/// </remarks>
		public void Add(string category) {
			Count++;
			if (Stats.TryGetValue(category, out int count)) {
				Stats[category] = count + 1;
			}
			else Stats.Add(category, 1);
		}
	
		/// <summary>
		/// Returns the count of the specified category in the dataset.
		/// </summary>
		/// <param name="category">The category to get the count of.</param>
		/// <returns>The count of the specified category in the dataset if it exists;
		/// otherwise, 0.</returns>
		public int GetStats(string category) {
			if (Stats.TryGetValue(category, out int count)) {
				return count;
			}

			return 0;
		}

		/// <summary>
		/// Returns the probability of the specified category in the dataset.
		/// </summary>
		/// <param name="category">The category to get the probability of.</param>
		/// <returns>The probability of the specified category in the dataset
		/// if it exists; otherwise, 0.0.</returns>
		public double GetProbability(string category) => GetStats(category) / (double)Count;

		/// <summary>
		/// Returns the probability of the specified category in the dataset.
		/// </summary>
		/// <param name="category">The category to get the probability of.</param>
		/// <returns>The probability of the specified category in the dataset
		/// if it exists; otherwise, 0.0.</returns>
		public bool Contains(string category) => Stats.ContainsKey(category);
	}
}
