namespace Santa_Gifts_API.Tools;
/// <summary>
/// This static class provides methods for loading sets of words from text files.
/// </summary>
public static class Lexicon {
	/// <summary>
	/// Loads the words from a file and returns a
	/// HashSet of the words.
	/// </summary>
	/// <param name="fileName">The name of the file to read from.</param>
	/// <returns>A HashSet of the words from the file.</returns>
	public static HashSet<string> Load(string fileName) {
		HashSet<string> data = [];
		string path = $"./data/lexicon/{fileName}.txt";

		using StreamReader r = new(path);
		string? line;

		while ((line = r.ReadLine()) != null) {
			data.Add(line);
		}

		return data;
	}
}
