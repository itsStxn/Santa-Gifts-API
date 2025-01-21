using Santa_Gifts_API.Core;
using Santa_Gifts_API.Tools;

namespace Santa_Gifts_API.AI.Models;
/// <summary>
/// Provides a class for implementing a naive Bayes classifier.
/// </summary>
/// <remarks>
/// This class uses a list of matrix factorization objects to calculate the conditional probabilities of each word given each category.
/// These probabilities are then used to calculate the posterior probability of each category given a new review.
/// </remarks>
public class NaiveBayes {
	/// <summary>
	/// The list of all input data.
	/// </summary>
	/// <value>The data.</value>
	/// <remarks>This list is used to calculate the prior probabilities of each category.</remarks>
	protected readonly List<string[]> Data = [];

	/// <summary>
	/// The list of matrix factorization objects.
	/// </summary>
	/// <value>The recommenders.</value>
	/// <remarks>This list is used to calculate the conditional probabilities of each word given each category.</remarks>
	protected readonly Dictionary<string, MatrixFactorization> Models = [];
	
	/// <summary>
	/// The main category used for classification.
	/// </summary>
	/// <value>The main category.</value>
	private readonly string MainCategory = "main_category";

	/// <summary>
	/// The list of word statistics.
	/// </summary>
	/// <value>The word stats.</value>
	/// <remarks>This list is used to calculate the prior probabilities of each category.</remarks>
	private readonly Dictionary<string, Categories> WordStats = [];

	/// <summary>
	/// The list of column names and their indices.
	/// </summary>
	/// <value>The columns.</value>
	/// <remarks>This list is used to quickly look up the position of each column in the data.</remarks>
	protected readonly Dictionary<string, int> Cols = [];

	/// <summary>
	/// The sentence object used to lemmatize words.
	/// </summary>
	/// <value>The sentence.</value>
	public Sentence Sentence { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Santa_Gifts_API.Core.NaiveBayes"/> class.
	/// </summary>
	/// <param name="sentence">The sentence string to analyze.</param>
	/// <remarks>
	/// This constructor reads the dataset string line by line and creates a matrix factorization object for each category.
	/// </remarks>
	/// Initializes a new instance of the <see cref="NaiveBayes"/> class.
	public NaiveBayes(string sentence) {
		Sentence = new(new(), sentence);
		string dataset = Dataset.Load();
		GetData(dataset);
	}

	/// <summary>
	/// Reads the dataset string line by line and creates a matrix factorization object for each category.
	/// It also creates a dictionary of word statistics for each category.
	/// </summary>
	/// <param name="dataset">The dataset string.</param>
	private void GetData(string dataset) {
		using StringReader reader = new(dataset);
		GetHeader(reader.ReadLine());
		string? line;

		while ((line = reader.ReadLine()) != null) {
			string[] row = line.Split('\t');
			Data.Add(row);
			UpdateModels(row);
		}
	}

	/// <summary>
	/// Parses the header line of the dataset and populates the column indices.
	/// </summary>
	/// <param name="header">The header line of the dataset, with column names separated by tabs.</param>
	/// <remarks>
	/// This method splits the header string by tabs and adds each column name
	/// along with its index to the Cols dictionary for quick lookup.
	/// </remarks>
	private void GetHeader(string? header) {
		string[] cols = header?.Split('\t') 
			?? throw new ArgumentNullException(nameof(header), "Header is null.");
		
		for (int i = 0; i < cols.Length; i++) {
			Cols.Add(cols[i], i);
		}
	}

	/// <summary>
	/// Calculates the score for a field in a row by summing the word (lemma) values of all words in the field.
	/// </summary>
	/// <param name="row">The row to evaluate.</param>
	/// <param name="i">The index of the field to evaluate.</param>
	/// <returns>The score for the field.</returns>
	/// <remarks>
	/// This method splits the field into words and adds the word value of each word to the score if it exists in the sentence's word values.
	/// This method also keeps track of the consecutive streak of interested words.
	/// </remarks>
	private double EvaulateField(string[] row, int i) {
		HashSet<string> seen = [];
		double score = 0;
		int streak = 1;

		foreach (string token in row[i].ToLower().Split(' ')) {
			string lemma = Sentence?.Dict.Lemmatize(token)
				?? throw new InvalidOperationException("Sentence is null.");
			
			AddWordStats(lemma, row[Cols[MainCategory]]);

			if (Sentence.WordValues.TryGetValue(lemma, out double value)) {
				if (seen.Add(lemma)) score += value * streak++;
			}
			else streak = 1;
		}

		return score;
	}

	/// <summary>
	/// Calculates the adjusted rating of a product based on its rating and the number of reviews.
	/// </summary>
	/// <param name="rating">The rating of the product.</param>
	/// <param name="ratings">The number of reviews of the product.</param>
	/// <returns>The adjusted rating.</returns>
	/// <remarks>
	/// The adjusted rating is calculated as the product of the rating and a factor that decreases
	/// with the number of reviews. The factor is given by 1 - Exp(-0.02 * count), where count is the
	/// number of reviews. This factor is used to give more weight to products with more reviews.
	/// </remarks>
	private static double AdjustedRating(string rating, string ratings) {
		if (double.TryParse(rating, out double score) && int.TryParse(ratings, out int count)) {
			return score * (1 - Math.Exp(-0.02 * count));
		}

		return 0;
	}

	/// <summary>
	/// Adds the category to the word statistics dictionary if the word already exists.
	/// Otherwise, creates a new word statistics dictionary with the given category.
	/// </summary>
	/// <param name="word">The word to add to the word statistics dictionary.</param>
	/// <param name="category">The category to add to the word statistics dictionary.</param>
	/// <remarks>This method is used to populate the word statistics dictionary with the categories of each word in the dataset.</remarks>
	private void AddWordStats(string word, string category) {
		if (WordStats.TryGetValue(word, out var categories)) {
			categories.Add(category);
		} 
		else {
			WordStats.Add(word, new Categories(category));
		}
	}

	/// <summary>
	/// Calculates the score of a row by summing the word values of each field and adding the adjusted rating.
	/// </summary>
	/// <param name="row">The row to evaluate.</param>
	/// <returns>The score of the row.</returns>
	/// <remarks>
	/// The row score is calculated as 60% of the sum of the word values of each of the first three fields
	/// and 40% of the adjusted rating. The adjusted rating is calculated as the rating multiplied by a factor
	/// that decreases with the number of reviews, given by 1 - Exp(-0.02 * count).
	/// </remarks>
	private double GetRowScore(string[] row) {
		double score = 0;
		for (int i = 0; i < 3; i++) {
			score += EvaulateField(row, i);
		}

		string rating = row[Cols["ratings"]];
		string reviews = row[Cols["no_of_ratings"]];

		score *= 0.6;
		score += AdjustedRating(rating, reviews) * 0.4;

		return score;
	}

	/// <summary>
	/// Updates the recommender for the given category with the score of the row.
	/// </summary>
	/// <param name="row">The row to update the recommender for.</param>
	/// <remarks>
	/// This method calculates the score of the row by summing the word values of each of its first three fields
	/// and adding the adjusted rating. The adjusted rating is calculated as the rating multiplied by a factor
	/// that decreases with the number of reviews, given by 1 - Exp(-0.02 * count).
	/// The score is then added to the recommender for the category of the row.
	/// If the category does not exist in the recommender, it is added with the given score.
	/// </remarks>
	private void UpdateModels(string[] row) {
		int n = Data.Count - 1;
		double rating = GetRowScore(row);
		string category = row[Cols[MainCategory]];

		if (Models.TryGetValue(category, out MatrixFactorization? model)) {
			model.AddScore(rating, n);
		}
		else {
			model = new MatrixFactorization(rating, n, Sentence.Polarity);
			Models.Add(category, model);
		}
	}

	/// <summary>
	/// Returns a stack of classes with the highest probability based on the words in the sentence.
	/// If the sentence has a positive polarity, the method returns the top-scoring classes.
	/// If the sentence has a negative polarity, the method returns a random stack of classes that do not appear
	/// in the top-scoring classes.
	/// </summary>
	/// <returns>A stack of classes with the highest probability.</returns>
	protected Stack<string> Classify() {
		if (IsEverything()) return RandomSuggest();

		Ranking<string> rank = new();
		int n = Sentence.Targets.Count;

		foreach (string category in Models.Keys) {
			double proba = CalculateProbability(Sentence.Targets, category);
			rank.InOut(category, proba, n);
		}

		Stack<string> classes = rank.Drop();
		return Sentence.Polarity < 0 ? RandomSuggest(rank.Dropped) : classes;
	}

	/// <summary>
	/// Determines whether the targets of the sentence contain the strings "everything" or "anything".
	/// </summary>
	/// <returns>True if the sentence contains "everything" or "anything"; otherwise, false.</returns>
	private bool IsEverything() {
		return Sentence.Targets.Contains("everything") || Sentence.Targets.Contains("anything");
	}

	/// <summary>
	/// Suggests a stack of categories to be recommended that are not in the excluded set.
	/// </summary>
	/// <param name="excluded">The optional set of categories to exclude from the suggestion.</param>
	/// <returns>A stack of category names that are not in the excluded set.</returns>
	/// <remarks>
	/// The number of categories suggested is equal to the number of targets in the sentence.
	/// The categories are randomly selected from the trained dataset, excluding the categories in the excluded set.
	/// </remarks>
	private Stack<string> RandomSuggest(HashSet<string>? excluded=null) {
		excluded ??= [];

		int n = Sentence.Targets.Count;
		Stack<string> stack = new();
		Random random = new();

		while (stack.Count < n) {
			string category = Models.Keys.ElementAt(random.Next(Models.Count));
			if (!excluded.Contains(category)) {
				stack.Push(category);
			}
		}

		return stack;
	}

	/// <summary>
	/// Calculates the logarithmic probability of a category given a hashset of targets.
	/// </summary>
	/// <param name="targets">The set of targets to calculate the probability for.</param>
	/// <param name="category">The category to calculate the probability for.</param>
	/// <param name="soothing">The smoothing factor to use.</param>
	/// <returns>The logarithmic probability of the category given the targets.</returns>
	/// <remarks>
	/// The logarithmic probability is calculated as the logarithm of the prior probability of the category
	/// multiplied by the product of the logarithmic conditional probabilities of each target given the category.
	/// The logarithmic conditional probability of a target given a category is calculated as the logarithm of the
	/// probability of the target in the category divided by the probability of the target in the dataset.
	/// If a target does not exist in the dataset, its logarithmic conditional probability is set to -6.
	/// </remarks>
	private double CalculateProbability(HashSet<string> targets, string category, double soothing=1e-6) {
		double rows = Models[category].Count;
		double categoryProba = rows / Data.Count;
		double logProba = Math.Log(categoryProba);

		foreach (string target in targets) {
			if (WordStats.TryGetValue(target, out var cat) && cat.Contains(category)) {
				double weight = cat.GetStats(category) / rows;
				double proba = cat.GetProbability(category);
				
				weight = Math.Max(weight, soothing);
				logProba += Math.Log(proba) + Math.Log(weight);
			}
			else logProba += Math.Log(soothing);
		}

		return logProba;
	}
}
