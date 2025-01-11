using Santa_Gifts_API.AI.Models;
using Santa_Gifts_API.DTOs;

namespace Santa_Gifts_API.AI.Services;
/// <summary>
/// Represents a recommender for items in a sentence.
/// </summary>
public class RecommenderService() : IRecommenderService {
	/// <summary>
	/// Recommends the top N items to the user that have not been rated yet,
	/// given the sentence that the user is currently typing.
	/// </summary>
	/// <param name="sentence">The sentence that the user is currently typing.</param>
	/// <returns>A list of dictionaries, where each dictionary represents an item
	/// with column names as keys and corresponding values in the row as dictionary values.</returns>
	/// <remarks>
	/// This method classifies the sentence with the naive Bayes classifier and then
	/// iterates over the categories in descending order of their probabilities.
	/// For each category, it recommends the top N unrated items using the matrix
	/// factorization model and adds them to the list.
	/// </remarks>
	public Recommendations Recommend(string sentence) {
		Recommender model = new(sentence);
		return model.Recommend();
	}
}
