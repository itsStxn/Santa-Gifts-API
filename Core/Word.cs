namespace Santa_Gifts_API.Core {
	/// <summary>
	/// Represents a word and its associated details such as its value,
	/// parts of speech, and synonyms.
	/// </summary>
	/// <remarks>
	/// This class provides functionality to manage the parts of speech
	/// and synonyms associated with a word. It allows adding new parts
	/// of speech and synonyms and retrieving descriptions of the word's
	/// types.
	/// </remarks>
	public class Word {
		/// <summary>
		/// Gets or sets the value of the word. The value is modified to be trimmed,
		/// lower-cased, and underscores replaced with spaces.
		/// </summary>
		private string Value = string.Empty;

		/// <summary>
		/// Gets or sets the frequencies of the word, which is a dictionary mapping
		/// parts of speech to their respective frequency of occurrence.
		/// </summary>
		private Dictionary<string, double> _frequencies = [];

		/// <summary>
		/// Gets or sets the frequencies of the word, which is a dictionary mapping
		/// parts of speech to their respective frequency of occurrence.
		/// </summary>
		public Dictionary<string, double> Frequencies => _frequencies;

		/// <summary>
		/// Gets or sets the details of the word, which is a dictionary mapping parts of speech
		/// to their corresponding synonyms.
		/// </summary>
		protected Dictionary<string, List<string>> Details = [];

		/// <summary>
		/// Creates a new Word with the given value.
		/// The value is modified to be trimmed,
		/// lower-cased, and underscores replaced with spaces.
		/// </summary>
		/// <param name="value">The value of the Word to create.</param>
		public Word(string value) {
			Set(value);
		}

		/// <summary>
		/// Sets the value of the word. The value is modified to be trimmed,
		/// lower-cased, and underscores replaced with spaces.
		/// </summary>
		/// <param name="value">The value to set for the word.</param>
		private void Set(string value) {
			Value = value
			.Trim()
			.ToLower()
			.Replace("_", " ");
		}

		/// <summary>
		/// Returns an array of strings describing the word type.
		/// </summary>
		/// <returns>An array of strings describing the type of the word.</returns>
		public string[] Describe() => [..Frequencies.Keys];

		/// <summary>
		/// Checks if the given type of part of speech exists for this word.
		/// </summary>
		/// <param name="type">The type of part of speech to check.</param>
		/// <returns>True if the given type exists, false otherwise.</returns>
		public bool IsType(string type) => Details.ContainsKey(type);

		/// <summary>
		/// Returns a list of synonyms for the given part of speech type.
		/// </summary>
		/// <param name="type">The type of the part of speech to get synonyms for.</param>
		/// <returns>A list of synonyms for the given part of speech type.</returns>
		/// <remarks>
		/// If the given type does not exist, an empty list is returned.
		/// </remarks>
		public List<string> Synonyms(string type) {
			if (Details.TryGetValue(type, out var synonyms)) {
				return synonyms;
			}

			return [];
		}

		/// <summary>
		/// Adds a part of speech type to the word's details.
		/// </summary>
		/// <param name="type">The type of the part of speech to add.</param>
		public void AddPartOfSpeech(string type) => Details.Add(type, []);
		
		/// <summary>
		/// Adds a synonym to the last added part of speech.
		/// </summary>
		/// <param name="synonym">The synonym to add.</param>
		/// <remarks>
		/// If no part of speech have been added, the synonym is not added.
		/// </remarks>
		public void AddSynonym(string synonym) {
			if (Details.Keys.Count == 0) return;
			Details[Details.Keys.Last()].Add(synonym);
		}

		/// <summary>
		/// Adds a synonym to the given part of speech type.
		/// </summary>
		/// <param name="synonym">The synonym to add.</param>
		/// <param name="type">The type of the part of speech to add the synonym to.</param>
		/// <remarks>
		/// If the given type does not exist, it is added and the synonym is added to it.
		/// </remarks>
		public void AddSynonym(string synonym, string type) {
			if (Details.TryGetValue(type, out var synonyms)) {
				synonyms.Add(synonym);
			} 
			else {
				AddPartOfSpeech(type);
				AddSynonym(synonym);
			}
		}

		/// <summary>
		/// Adds a part of speech type with its synonyms and frequency to the word's details.
		/// Type's frequencies are sorted in descending order.
		/// The word's value is part of the synonyms.
		/// </summary>
		/// <param name="type">The type of the part of speech to add.</param>
		/// <param name="frequency">The frequency of the part of speech to add.</param>
		/// <param name="synonyms">The synonyms to add to the part of speech.</param>
		public void AddDetails(string type, double frequency, string[] synonyms) {
			Details.Add(type, [..synonyms]);
			Frequencies.Add(type, frequency);
			SortFrequencies();
		}

		/// <summary>
		/// Sorts the frequencies of the word in descending order.
		/// </summary>
		private void SortFrequencies() {
			_frequencies = Frequencies
			.OrderByDescending(r => r.Value)
			.ToDictionary(r => r.Key, r => r.Value);
		}

		/// <summary>
		/// Gets a string representation of the word.
		/// </summary>
		/// <returns>A string representation of the word.</returns>
		public override string ToString() => Value;
	}
}
