using Santa_Gifts_API.DTOs;

namespace Santa_Gifts_API.AI.Models;
/// <summary>
/// Represents a recommender for items in a sentence.
/// </summary>
public class Recommender(string sentence) : NaiveBayes(sentence) {
	/// <summary>
	/// Recommends the top N items to the user that have not been rated yet.
	/// </summary>
	/// <returns>A list of dictionaries, where each dictionary represents an item with column names as keys and corresponding values in the row as dictionary values.</returns>
	/// <remarks>
	/// This method classifies the sentence with the naive Bayes classifier and then iterates over the categories in descending order of their probabilities.
	/// For each category, it recommends the top N unrated items using the matrix factorization model and adds them to the list.
	/// </remarks>
	public Recommendations Recommend() {
		Recommendations list = new();
		var classes = Classify();
		
		while (classes.Count > 0) {
			var model = Models[classes.Pop()];
			var recommendations = model.Recommend();

			while (recommendations.Count > 0) {
				var row = Data[recommendations.Pop()];
				list.Items.Add(RowToDictionary(row));
			}
		}

		return list;
	}

	/// <summary>
	/// Converts a row of data into a dictionary with column names as keys.
	/// </summary>
	/// <param name="row">The row of data to convert.</param>
	/// <returns>A dictionary with column names as keys and corresponding values in the row as dictionary values.</returns>
	private Dictionary<string, string> RowToDictionary(string[] row) {
		Dictionary<string, string> dict = [];
		
		int col = 0;
		foreach (string column in Cols.Keys) {
			dict.Add(column, row[col++]);
		}

		return dict;
	}
}
