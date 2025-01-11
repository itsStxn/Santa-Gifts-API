using Santa_Gifts_API.Tools;

namespace Santa_Gifts_API.Core;
/// <summary>
/// The Vocabulary class is responsible for managing the
/// dictionary of words, their synonyms, and types.
/// </summary>
public class Vocabulary {
	/// <summary>
	/// A dictionary mapping words to their corresponding <see cref="Word"/> objects.
	/// </summary>
	private Dictionary<string, Word> Words { get; set; }

	/// <summary>
	/// A dictionary mapping words to their lemmas.
	/// </summary>
	private Dictionary<string, string> Lemmas { get; set; }

	/// <summary>
	/// A dictionary mapping words to their polarities.
	/// </summary>
	private Dictionary<string, double> Polarities { get; set; }

	/// <summary>
	/// A set of words that are stopwords.
	/// </summary>
	private HashSet<string> Stopwords { get; set; }
	
	/// <summary>
	/// A set of words that are shifters.
	/// </summary>
	private HashSet<string> Shifters { get; set; }

	/// <summary>
	/// A set of words that are pronouns.
	/// </summary>
	private HashSet<string> Pronouns { get; set; }

	/// <summary>
	/// A set of words that are determiners.
	/// </summary>
	private HashSet<string> Determiners { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Vocabulary"/> class.
	/// Loads words, lemmas, polarities, pronouns, shifters, stopwords, and determiners
	/// from their respective data sources.
	/// </summary>
	public Vocabulary() {
		Words = WordNet.Load();
		Lemmas = Lemma.Load();
		Polarities = Polarity.Load();
		Pronouns = Lexicon.Load("pronouns");
		Shifters = Lexicon.Load("shifters");
		Stopwords = Lexicon.Load("stopwords");
		Determiners = Lexicon.Load("determiners");
	}

	/// <summary>
	/// Searches for the Word object associated with the given input string.
	/// </summary>
	/// <param name="input">The word to search for.</param>
	/// <returns>The Word object if found; otherwise, null.</returns>
	public Word? Search(string input) {
		if (Words.TryGetValue(input.ToLower(), out var word)) {
			return word;
		}

		return null;
	}

	/// <summary>
	/// Returns the Word object associated with the given input string.
	/// If the word is not found in the dictionary, a new Word object is created
	/// with the given input string as its value and the defaultType as its type.
	/// The input string itself is added as a synonym of the Word object.
	/// </summary>
	/// <param name="input">The word to search for.</param>
	/// <param name="defaultType">The default type to assign if the word is not found.</param>
	/// <returns>The Word object associated with the given input string.</returns>
	public Word Search(string input, string defaultType) {
		if (!Words.TryGetValue(input.ToLower(), out var word)) {
			word = new(input);
			word.AddPartOfSpeech(defaultType);
			word.AddSynonym(word.ToString());
		}

		return word;
	}

	/// <summary>
	/// Returns the lemma of the input word if it exists in the dictionary;
	/// otherwise, returns the input word itself.
	/// </summary>
	/// <param name="input">The word to be lemmatized.</param>
	/// <returns>The lemma of the input word if found; otherwise, the input word itself.</returns>
	public string Lemmatize(string input) {
		input = input.ToLower();
		if (Lemmas.TryGetValue(input, out var lemma)) {
			return lemma;
		}

		return input;
	}

	/// <summary>
	/// Returns the polarity of the input word if it exists in the dictionary;
	/// otherwise, returns 0.0.
	/// </summary>
	/// <param name="input">The word to get the polarity of.</param>
	/// <returns>The polarity of the input word if found; otherwise, 0.0.</returns>
	public double GetPolarity(string input) {
		input = input.ToLower();
		if (Polarities.TryGetValue(input, out var polarity)) {
			return polarity;
		}
		
		return 0.0;
	}

	/// <summary>
	/// Determines whether the specified word is a shifter.
	/// </summary>
	/// <param name="input">The word to check.</param>
	/// <returns>True if the word is a shifter; otherwise, false.</returns>
	public bool IsShifter(string input) => Shifters.Contains(input.ToLower());

	/// <summary>
	/// Determines whether the specified word is a stopword.
	/// </summary>
	/// <param name="input">The word to check.</param>
	/// <returns>True if the word is a stopword; otherwise, false.</returns>
	public bool IsStopword(string input) => Stopwords.Contains(input.ToLower());

	/// <summary>
	/// Determines whether the specified word is a pronoun.
	/// </summary>
	/// <param name="input">The word to check.</param>
	/// <returns>True if the word is a pronoun; otherwise, false.</returns>
	public bool IsPronoun(string input) => Pronouns.Contains(input.ToLower());

	/// <summary>
	/// Determines whether the specified word is a determiner.
	/// </summary>
	/// <param name="input">The word to check.</param>
	/// <returns>True if the word is a determiner; otherwise, false.</returns>
	public bool IsDeterminer(string input) => Determiners.Contains(input.ToLower());
}
