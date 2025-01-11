using Santa_Gifts_API.Core;

namespace Santa_Gifts_API.Tools {
	/// <summary>
	/// Provides a class for loading synonyms and types from a WordNet CSV file.
	/// </summary>
	public static class WordNet {
		/// <summary>
		/// Loads synonyms, frequencies and types and returns a dictionary mapping each word to a Word object.
		/// </summary>
		/// <returns>A dictionary mapping each word to a Word object.</returns>
		public static Dictionary<string, Word> Load() {
			Dictionary<string, Word> data = [];
			string path = "./data/lexicon/synonyms.txt";

			using StreamReader r = new(path);
			r.ReadLine();
			string? line;

			while ((line = r.ReadLine()) != null) {
				string[] record = line.ToLower().Split(',');
				var freq = Convert.ToDouble(record[3]);
				var synonyms = record[2].Split(';');
				string text = record[0];
				string type = record[1];

				if (!data.TryGetValue(text, out Word? word)) {
					word = new Word(text);
					data.Add(text, word);
				}
				word.AddDetails(type, freq, synonyms);
			}

			return data;
		}
	}
}
