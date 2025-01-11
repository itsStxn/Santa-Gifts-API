namespace Santa_Gifts_API.Tools;
/// <summary>
/// This static class provides methods for loading lemmas from a text file
/// and returning a dictionary mapping each word to its lemma.
/// </summary>
public static class Lemma {
	/// <summary>
	/// Loads the lemmas from a file and returns a dictionary mapping each word to its lemma.
	/// </summary>
	/// <returns>A dictionary mapping each word to its lemma.</returns>
	public static Dictionary<string, string> Load() {
		var data = new Dictionary<string, string>();
		string path = "./data/lexicon/lemmas.txt";

		using StreamReader r = new(path);
		string? line;

		while ((line = r.ReadLine()) != null) {
			string[] record = line.Split(',');
			string[] words = record[1].Split(';');
			string lemma = record[0];

			foreach (string word in words) {
				data.TryAdd(word, lemma);
			}
		}

		return data;
	}
}
