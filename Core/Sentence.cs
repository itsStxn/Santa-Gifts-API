using System.Text.RegularExpressions;

namespace Santa_Gifts_API.Core {
	/// <summary>
	/// Represents a sentence and the associated vocabulary, type map, and list of targets.
	/// </summary>
	public partial class Sentence {
		/// <summary>
		/// Gets the vocabulary associated with the sentence.
		/// </summary>
		/// <value>The vocabulary associated with the sentence.</value>
		public Vocabulary Dict { get; }
		
		/// <summary>
		/// Gets or sets the type map for the sentence. The type map is a dictionary
		/// that maps each word type to the set of word types that can follow it.
		/// </summary>
		/// <value>The dictionary of word types and the set of word types that can follow them.</value>
		private readonly Dictionary<string, HashSet<string>> TypeMap = [];

		/// <summary>
		/// Gets the dictionary of all possible synonyms of the input words
		/// and their score based on their role in the sentence.
		/// </summary>
		/// <value>The dictionary of words and their score.</value>
		private readonly Dictionary<string, double> _wordValues = [];

		/// <summary>
		/// Gets the dictionary of all possible synonyms of the input words
		/// and their score based on their role in the sentence.
		/// </summary>
		/// <value>The dictionary of words and their score.</value>
		public Dictionary<string, double> WordValues => _wordValues;

		/// <summary>
		/// Gets the hashset of targets associated with the sentence.
		/// </summary>
		/// <value>The hashset of targets associated with the sentence.</value>
		private readonly HashSet<string> _targets = [];
		
		/// <summary>
		/// Gets the hashset of targets associated with the sentence.
		/// </summary>
		/// <value>The hashset of targets associated with the sentence.</value>
		public HashSet<string> Targets => _targets;

		/// <summary>
		/// Gets the set of allowed types for the sentence.
		/// </summary>
		/// <value>The set of allowed types for the sentence.</value>
		public string[] Allowed { get; set; }

		/// <summary>
		/// Gets the list of tokens (words) in the sentence.
		/// </summary>
		/// <value>The list of tokens (words) in the sentence.</value>
		public string[] Tokens { get; set; }

		/// <summary>
		/// Gets the polarity of the sentence.
		/// </summary>
		/// <value>The polarity of the sentence.</value>
		private double _polarity = 1;
		
		/// <summary>
		/// Gets the polarity of the sentence.
		/// </summary>
		/// <value>The polarity of the sentence.</value>
		public double Polarity => _polarity;

		[GeneratedRegex(@"[^\w'\s\-\/]")]
		private static partial Regex _regex();

		/// <summary>
		/// Initializes a new instance of the Sentence class with the given vocabulary.
		/// Defines the type map and reads the sentence.
		/// Splits the sentence into tokens.
		/// </summary>
		/// <param name="dictionary">The dictionary of words to use for the sentence.</param>
		/// <param name="sentence">The sentence to initialize.</param>
		public Sentence(Vocabulary dictionary, string sentence) {
			Allowed = ["noun", "verb", "adjective", "satellite", "determiner", "pronoun", "shifter"];
			Tokens = Tokenize(sentence);
			Dict = dictionary;
			DefineRules();
			Analyze();
		}
		
		/// <summary>
		/// Initializes the type map for the sentence.
		/// </summary>
		/// <remarks>
		/// This method sets up the type map for the sentence. The type map
		/// is a dictionary that maps each word type (noun, verb, adjective,
		/// satellite, determiner, pronoun, shifter) to the set of word types
		/// that can follow it. For example, the set of word types that can
		/// follow a noun is { noun, verb, adjective, satellite, shifter,
		/// determiner }.
		/// </remarks>
		private void DefineRules() {
			TypeMap.Add(string.Empty, ["noun", "adjective", "satellite", "determiner", "pronoun"]);
			TypeMap.Add("noun", ["noun", "verb", "adjective", "satellite", "shifter", "determiner"]);
			TypeMap.Add("determiner", ["noun", "adjective", "satellite", "determiner"]);
			TypeMap.Add("verb", ["satellite", "adjective", "noun", "determiner"]);
			TypeMap.Add("satellite", ["noun", "adjective", "satellite"]);
			TypeMap.Add("adjective", ["noun", "adjective", "satellite"]);
			TypeMap.Add("pronoun", ["verb", "shifter"]);
			TypeMap.Add("shifter", ["verb"]);
		}

		/// <summary>
		/// Reads the given sentence and determines the sentiment using the
		/// vocabulary provided. The sentiment is determined by finding the
		/// first verb in the sentence and then determining the sentiment of
		/// the verb and the words that follow it. The sentiment of the verb
		/// and the words that follow it are then combined to determine the
		/// overall sentiment of the sentence.
		/// </summary>
		/// <param name="sentence">The sentence to analyze.</param>
		/// <exception cref="InvalidDataException">
		/// Thrown if the sentence has less than 3 words.
		/// </exception>
		/// <exception cref="InvalidDataException">
		/// Thrown if the sentence has no verb.
		/// </exception>
		private void Analyze() {
			if (Tokens.Length < 3) {
				throw new InvalidDataException("Crazy input!");
			}

			int verbIndex = GetVerbIndex();
			if (verbIndex == -1) {
				throw new InvalidDataException("Crazy input!");
			}

			string verb = Tokens[verbIndex];
			_polarity *= Dict.GetPolarity(verb);
			ProcessTokens(verbIndex);
		}

		/// <summary>
		/// Processes the tokens in the sentence, adding word values to the
		/// sentence's word values dictionary. The word values are determined
		/// based on the type of word and whether or not the word is a target.
		/// </summary>
		/// <param name="verbIndex">The index of the verb in the sentence.</param>
		private void ProcessTokens(int verbIndex) {
			for (int i = 0; i < Tokens.Length; i++) {
				string token = Tokens[i];
				
				if (i != verbIndex && token.Contains(';')) {
					string[] record = token.Split(';');

					if (record.Length == 2) {
						string word = record[0];
						string type = record[1];
						bool isTarget = i > verbIndex && type == "noun";
						AddWordValues(word, type, isTarget);
					}
				}
			}
		}

		/// <summary>
		/// Adds the given word and its synonyms to the sentence's word values
		/// dictionary. The word values are determined based on the type of word,
		/// and whether or not the word is a target.
		/// Original words are given the highest score.
		/// </summary>
		/// <param name="token">The word to add.</param>
		/// <param name="type">The type of the word.</param>
		/// <param name="isTarget">True if the word is a target; otherwise, false.</param>
		/// <param name="max">The maximum score for the word.</param>
		private void AddWordValues(string token, string type, bool isTarget=false, int max=5) {
			var word = Dict.Search(token)
				?? throw new InvalidDataException("Crazy input!");
				
			if (isTarget) Targets.Add(word.ToString());

			double factor;
			if (isTarget) factor = 0.4;
			else factor = type == "adjective" ? 0.1 : 0.2;

			foreach (string synonym in word.Synonyms(type)) {
				if (!WordValues.ContainsKey(synonym)) {
					factor = isTarget && synonym == token ? 1 : factor;
					WordValues.Add(synonym, factor * max);
				}
			}
		}

		/// <summary>
		/// Splits the given sentence into tokens, where a token is a sequence
		/// of alphanumeric characters, apostrophes, spaces, hyphens, or
		/// forward slashes. All other characters are ignored.
		/// </summary>
		/// <param name="sentence">The sentence to tokenize.</param>
		/// <returns>An array of strings, where each string is a token in the
		/// sentence.</returns>
		private static string[] Tokenize(string sentence) {
			sentence = _regex().Replace(sentence, string.Empty);
			return sentence.Split(' ');
		}

		/// <summary>
		/// Returns a dictionary of word types and their frequencies given a word.
		/// If the word is a shifter, determiner, pronoun, or stopword, the
		/// dictionary will contain the word type mapped to -1.0. If the word is
		/// not recognized, an empty dictionary is returned.
		/// </summary>
		/// <param name="token">The word to get the word types for.</param>
		/// <returns>A dictionary of word types and their frequencies.</returns>
		private Dictionary<string, double> GetWordTypes(string token) {
			Dictionary<string, double> dict = [];

			if (Dict.IsShifter(token)) {
				dict.Add("shifter", -1);
			}
			else if (Dict.IsDeterminer(token)) {
				dict.Add("determiner", -1);
			}
			else if (Dict.IsPronoun(token)) {
				dict.Add("pronoun", -1);
			}
			else if (Dict.IsStopword(token)) {
				dict.Add("stopword", -1);
			}
			else {
				var word = Dict.Search(token);
				if (word != null) {
					dict = word.Frequencies;
				}
			}
			
			return dict;
		}

		/// <summary>
		/// Finds the index of the most likely verb in the sentence, starting from a given index.
		/// </summary>
		/// <param name="rules">A set of word types that are allowed to follow the current word.</param>
		/// <param name="freq">The frequency of the current word type.</param>
		/// <param name="verb">The current index of the verb found in the sentence.</param>
		/// <param name="i">The starting index from which to search for a verb.</param>
		/// <returns>The index of the verb if found; otherwise, -1.</returns>
		private int GetVerbIndex(HashSet<string>? rules=null, double freq=-1, int verb=-1, int i=0) {
			if (i >= Tokens.Length)
				return verb < Tokens.Length - 1 ? verb : -1;

			rules ??= TypeMap[string.Empty];

			string token = Tokens[i].ToLower();
			string lemma = Dict.Lemmatize(token);

			foreach (string word in new HashSet<string> { lemma, token }) {
				int index = ProcessWord(word, rules, freq, verb, i);
				if (index != -1) return index;
			}

			return -1;
		}

		/// <summary>
		/// Finds the index of the most likely verb in the sentence by processing a
		/// given word. The word is processed by getting its types, checking if each
		/// type is allowed to follow the current word, and then evaluating the type
		/// to determine if it is a verb.
		/// </summary>
		/// <param name="word">The word to process.</param>
		/// <param name="rules">A set of word types that are allowed to follow the current word.</param>
		/// <param name="freq">The frequency of the current word type.</param>
		/// <param name="verb">The current index of the verb found in the sentence.</param>
		/// <param name="i">The starting index from which to search for a verb.</param>
		/// <returns>The index of the verb if found; otherwise, -1.</returns>
		private int ProcessWord(string word, HashSet<string> rules, double freq, int verb, int i) {
			var types = GetWordTypes(word);

			foreach (var type in types) {
				if (rules.Contains(type.Key)) {
					var nextRules = TypeMap[type.Key];
					int index = EvaluateType(type, nextRules, freq, verb, i);

					if (index != -1) {
						SaveWord(word, type.Key, i);
						return index;
					} 
				}
				else if (!IsAllowed(types)) {
					return GetVerbIndex(rules, freq, verb, i+1);
				}
			}

			return -1;
		}

		/// <summary>
		/// Determines if any of the types in the given dictionary are allowed.
		/// </summary>
		/// <param name="types">The dictionary of types to check.</param>
		/// <returns>True if any of the types are in the Allowed set; otherwise, false.</returns>
		private bool IsAllowed(Dictionary<string, double> types) {
			return Allowed.Any(type => types.ContainsKey(type));
		}

		/// <summary>
		/// Saves the given word and type at the specified index in the
		/// sentence. The given word and type are used to determine how to
		/// save the word. If the type is "noun" or "adjective", the word
		/// is saved with a semicolon followed by the type. If the type
		/// is "shifter", the polarity of the sentence is set to -1. If the
		/// type is "verb", the word is saved without modification.
		/// </summary>
		/// <param name="word">The word to save.</param>
		/// <param name="type">The type of the word to save.</param>
		/// <param name="i">The index at which to save the word.</param>
		private void SaveWord(string word, string type, int i) {
			if (type == "noun" || type == "adjective") {
				Tokens[i] = $"{word};{type}";
			} 
			else if (type == "shifter") {
				_polarity = -1;
			}
			else if (type == "verb") {
				Tokens[i] = word;
			}
		}

		/// <summary>
		/// Evaluates the given word type to determine if it is a verb and returns the index of the verb
		/// in the sentence. If the type is not "verb" or its frequency is less than the current frequency,
		/// it proceeds to find the next verb index. If the current frequency is negative, it uses the type's
		/// frequency for further evaluation.
		/// </summary>
		/// <param name="type">The word type and its frequency to evaluate.</param>
		/// <param name="nextRules">A set of word types that are allowed to follow the current word.</param>
		/// <param name="freq">The frequency of the current word type.</param>
		/// <param name="verb">The current index of the verb found in the sentence.</param>
		/// <param name="i">The starting index from which to search for a verb.</param>
		/// <returns>The index of the verb if found; otherwise, -1.</returns>
		private int EvaluateType(KeyValuePair<string, double> type, HashSet<string> nextRules, double freq, int verb, int i) {
			if (type.Key != "verb" || type.Value < freq) {
				return GetVerbIndex(nextRules, freq, verb, i+1);
			} 
			else if (freq < 0) {
				return GetVerbIndex(nextRules, type.Value, i, i+1);
			}

			return -1;
		}
	}
}
