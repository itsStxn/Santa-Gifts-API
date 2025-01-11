using Santa_Gifts_API.DTOs;

namespace Santa_Gifts_API.AI.Services;
/// <summary>
/// Represents a recommender for items in a sentence.
/// </summary>
public interface IRecommenderService {

	/// <summary>
	/// Classifies the sentence into categories, and then recommends items
	/// for each category. The recommended items are returned as a Recommendations object
	/// </summary>
	/// <returns>A Recommendations object with the recommended items for each category.</returns>
	public Recommendations Recommend(string sentence);
}
