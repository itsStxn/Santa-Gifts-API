namespace Santa_Gifts_API.Tools;
/// <summary>
/// This static class provides methods for loading verb polarities from a file.
/// </summary>
public static class Polarity {
	/// <summary>
	/// Loads the verb polarities from a file and returns a dictionary mapping each verb to its polarity.
	/// </summary>
	/// <returns>A dictionary mapping each verb to its polarity.</returns>
	public static Dictionary<string, double> Load() {
		Dictionary<string, double> data = [];
		string path = "./data/lexicon/polarities.txt";

		using StreamReader r = new(path);
		string? line;

		while ((line = r.ReadLine()) != null) {
			string[] record = line.Split(',');
			string verb = record[0];
			double polarity = double.Parse(record[1]);
			
			data.Add(verb, polarity);
		}

		return data;
	}
}
