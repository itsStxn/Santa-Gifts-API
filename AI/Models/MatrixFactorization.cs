using Santa_Gifts_API.Core;

namespace Santa_Gifts_API.AI.Models;
/// <summary>
/// Provides a class for storing a matrix factorization model.
/// </summary>
/// <remarks>
/// The matrix factorization model is a list of scores and a list of item pointers.
/// The list of scores is the list of ratings for all items in the dataset.
/// The list of item pointers is a list of the item indices in the dataset.
/// Scores signs depend on the sentiment of the sentence parent.
/// </remarks>
public class MatrixFactorization {
	/// <summary>
	/// The list of scores in the matrix factorization model.
	/// </summary>
	/// <remarks>
	/// The list of scores is the list of ratings for all items in the dataset.
	/// </remarks>
	private readonly List<double> Scores = [];
	
	/// <summary>
	/// The list of item pointers in the matrix factorization model.
	/// </summary>
	/// <remarks>
	/// The list of item pointers is a list of the item indices in the dataset.
	/// </remarks>
	private readonly List<int> Pointers = [];
	
	/// <summary>
	/// The count of items in the matrix factorization model.
	/// </summary>
	/// <remarks>
	/// The count of items is the number of items in the dataset.
	/// </remarks>
	public int Count => Pointers.Count;
	
	/// <summary>
	/// The item factors in the matrix factorization model.
	/// </summary>
	/// <remarks>
	/// The item factors are the latent factors of the items in the dataset.
	/// </remarks>
	private double[,] ItemFactors = new double[0, 0];
	
	/// <summary>
	/// The user factors in the matrix factorization model.
	/// </summary>
	/// <remarks>
	/// The user factors are the latent factors of the users in the dataset.
	/// </remarks>
	private double[] UserFactors = [];
	
	/// <summary>
	/// The learning rate of the matrix factorization model.
	/// </summary>
	/// <remarks>
	/// The learning rate is the rate at which the gradient descent algorithm updates the model parameters.
	/// </remarks>
	private const double LearningRate = 0.01;
	
	/// <summary>
	/// The maximum number of iterations of the matrix factorization model.
	/// </summary>
	/// <remarks>
	/// The maximum number of iterations is the number of iterations of the gradient descent algorithm.
	/// </remarks>
	private const int MaxIterations = 100;
	
	/// <summary>
	/// The number of factors in the matrix factorization model.
	/// </summary>
	/// <remarks>
	/// The number of factors is the number of latent factors of the items in the dataset.
	/// </remarks>
	private const int Factors = 50;

	private bool IsPositive { get; set; }

	/// <summary>
	/// Initializes a new instance of the MatrixFactorization class with the specified score and index.
	/// </summary>
	/// <param name="score">The initial score to add to the list of scores.</param>
	/// <param name="index">The index associated with the initial score.</param>
	public MatrixFactorization(double score, int index, double polarity) {
		IsPositive = polarity >= 0;
		AddScore(score, index);
	}

	/// <summary>
	/// Adds the specified score and index to the scores and pointers lists, respectively.
	/// </summary>
	/// <param name="score">The score to add to the list of scores.</param>
	/// <param name="index">The index associated with the score to add to the list of indices.</param>
	public void AddScore(double score, int index) {
		Scores.Add(IsPositive ? score : -score);
		Pointers.Add(index);
	}

	/// <summary>
	/// Fills the user and item factors with random values.
	/// </summary>
	/// <remarks>
	/// This method initializes the user and item factors with random values.
	/// The user factors are initialized with a single random double for each factor.
	/// The item factors are initialized with a two-dimensional array of random double values,
	/// with the number of rows equal to the number of factors and the number of columns equal to the count.
	/// </remarks>
	private void FillFactors() {
		ItemFactors = new double[Factors, Count];
		UserFactors = new double[Factors];
		var r = new Random();

		for (int i = 0; i < UserFactors.Length; i++) {
			UserFactors[i] = r.NextDouble();
		}

		for (int row = 0; row < Factors; row++) {
			for (int col = 0; col < Count; col++) {
				ItemFactors[row, col] = r.NextDouble();
			}
		}
	}

	/// <summary>
	/// Predicts the score of the item with the specified index using the current user and item factors.
	/// </summary>
	/// <param name="item">The index of the item to predict the score of.</param>
	/// <returns>The predicted score of the item.</returns>
	/// <remarks>
	/// This method predicts the score of the specified item by summing the products of the corresponding
	/// user and item factors.
	/// </remarks>
	private double Predict(int item) {
		double score = 0;
		for (int k = 0; k < Factors; k++) {
			score += UserFactors[k] * ItemFactors[k, item];
		}
		
		return score;
	}

	/// <summary>
	/// Trains the matrix factorization model using stochastic gradient descent.
	/// </summary>
	/// <remarks>
	/// This method initializes the user and item factors, and iteratively updates them
	/// for a specified number of iterations. For each item in the dataset with a positive score,
	/// it calculates the prediction error and adjusts the factors using the learning rate.
	/// </remarks>
	private void Train() {
		FillFactors();
		for (int iter = 0; iter < MaxIterations; iter++) {
			for (int item = 0; item < Count; item++) {
				double score = Scores[item];

				if (score > 0) {
					double error = score - Predict(item);
					for (int k = 0; k < Factors; k++) {
						UserFactors[k] += LearningRate * error * ItemFactors[k, item];
						ItemFactors[k, item] += LearningRate * error * UserFactors[k];
					}
				}
			}
		}
	}

	/// <summary>
	/// Recommends the top N items to the user that have not been rated yet.
	/// </summary>
	/// <param name="top">The number of items to return. Defaults to 3.</param>
	/// <returns>A stack of item indices in descending order of predicted scores.</returns>
	/// <remarks>
	/// This method trains the model and then ranks all unrated items by their predicted scores.
	/// The top N items are returned in a stack, where the top item is the most highly recommended.
	/// </remarks>
	public Stack<int> Recommend(int top=3) {
		Train();
		Ranking<int> rank = new();
		for (int item = 0; item < Count; item++) {
			rank.InOut(Pointers[item], Predict(item), top);
		}

		return rank.Drop();
	}
}
