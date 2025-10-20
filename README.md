# Santa Gifts API

A local ASP.NET Web API that generates gift recommendations from a single input sentence. The system classifies target objects mentioned in the sentence into product categories (based on the Amazon Sales 2023 dataset) and returns recommended product rows using a weighted Naive Bayes classifier combined with a matrix factorization recommender.

This README documents the project structure, components, how to run the API locally, authentication and an example request / sample response.

---

## Table of contents

- Project summary
- High-level architecture
- Components and responsibilities
- Local setup and run instructions
- Configuration and environment variables
- API usage (endpoint, request examples for PowerShell and curl)
- Example successful response
- Notes, limitations and troubleshooting

---

## Project summary

- Language: C# (.NET 9)
- Purpose: Given a natural-language sentence, detect target entities and recommend relevant products from a preprocessed Amazon dataset.
- Techniques used: Weighted Naive Bayes for classification, Matrix Factorization for ranking/recommendation.

This project is intended to run locally and uses an API key header for a simple authentication layer.

---

## High-level architecture

- Program.cs
  - Application bootstrap, dependency registrations and Swagger setup.
  - Adds `IRecommenderService` implementation and API-key authentication scheme.

- Controllers
  - `RecommenderController` — single POST endpoint that accepts a sentence and returns recommendations. The controller requires the `MY-API-KEY` header.

- AI (services + models)
  - `IRecommenderService` / `RecommenderService` — service exposed to controllers.
  - `AI.Models.Recommender` — orchestration class that extends `NaiveBayes` and converts results into DTOs.
  - `AI.Models.NaiveBayes` — reads the product dataset, builds per-category matrix factorization models and computes class/posterior scores for sentence targets.
  - `AI.Models.MatrixFactorization` — small matrix-factorization training and predict implementation used to rank items for a category.

- Core
  - `Sentence`, `Vocabulary`, `Word`, `Categories`, `Ranking` — utilities for tokenizing, part-of-speech and role detection, lemmatization, polarity handling and ranking.

- Tools
  - Data-loading helpers: `Dataset` (loads gzipped CSV), `WordNet`, `Lemma`, `Polarity`, and `Lexicon` loaders.
  - `ApiKeyAuthenticationHandler` — simple header-based authentication.

- Data (in repo)
  - `data/products.csv.gz` — gzipped TSV dataset expected by the Naive Bayes model. The dataset header must include columns referenced by the code (for example: `title`, `description`, `price`, `ratings`, `no_of_ratings`, `main_category`, etc.).
  - `data/lexicon/*` — text resources used by the vocabulary (lemmas, polarities, synonyms and lexicon lists).

---

## Components and what they do (detailed)

- Vocabulary / Word / Lemma / Polarity / Lexicon
  - Load and expose linguistic resources used to tokenize and interpret the input sentence. Lemmas, synonyms, verb polarities and lists (pronouns, determiners, stopwords, shifters) are used to detect roles and sentiment.

- Sentence
  - Tokenizes input, discovers the verb and targets (nouns) that follow the verb, assigns role-based scores to words and computes sentence polarity.
  - Throws `InvalidDataException` for inputs that are too short or when no verb is found (the code uses error message text `Crazy input!`).

- NaiveBayes
  - Loads the product dataset (via `Dataset.Load()` which decompresses the gzipped file), builds per-category `MatrixFactorization` models and computes priors/conditional probabilities using word statistics.
  - Classification returns a stack of candidate categories for the sentence's targets. When polarity is negative a random suggestion path is used.

- MatrixFactorization
  - Lightweight matrix-factorization implementation used to produce a rank over items for a given category. It uses randomized initialization and SGD training over a short number of iterations and returns the top-N item indices.

- Recommender
  - Orchestrates classification and recommendations. Converts dataset rows into dictionaries and into the `Recommendations` DTO returned by the API.

- RecommenderController
  - Exposes POST /API/Recommender which accepts the sentence (FromBody string) and returns a `Recommendations` DTO on success.

---

## Local setup and run instructions

Prerequisites
- .NET 9 SDK installed and available in PATH
- The repository workspace (this project folder) available locally
- The dataset file expected at `./data/products.csv.gz` (gzipped TSV) and lexicon files at `./data/lexicon/`

Steps
1. Ensure the API key environment variable is set. The API uses an environment variable named `API_KEY`. In PowerShell:

   $env:API_KEY = 'your_secret_key'

   (Or set it permanently via Windows Environment Variables / system settings.)

2. Run the API locally from the project root. From PowerShell:

   dotnet run --project "Santa Gifts API.csproj"

   By default the project will start and Swagger UI will be available. Program.cs redirects `/` to `/swagger`.

3. Use the API as described below.

---

## Configuration

- API key: set the `API_KEY` environment variable to the same value you will send in the request header `MY-API-KEY`.
- Data files: `data/products.csv.gz` must exist and be a gzipped file containing a TSV where the header row is used to map column names in the code. The lexicon files live under `data/lexicon/` and are loaded at runtime.

---

## API usage

Endpoint
- POST /API/Recommender
- Auth: header `MY-API-KEY: {your_api_key}` is required
- Body: JSON string literal containing the sentence. Because the controller binds to `string` via [FromBody], the JSON payload must be a quoted string value.
- Response: 200 OK with JSON body matching the `Recommendations` DTO, or 400 on error with a basic error object.

PowerShell example (Invoke-RestMethod)

```powershell
Invoke-RestMethod -Uri "http://localhost:5291/API/Recommender" -Method Post -Headers @{ 'MY-API-KEY' = $env:API_KEY } -ContentType 'application/json' -Body '"I want a gift for my sister who likes painting"'
```

cURL example (bash/cmd)

```bash
curl -X POST "http://localhost:5291/API/Recommender" -H "Content-Type: application/json" -H "MY-API-KEY: your_key" -d '"I want a gift for my sister who likes painting"'
```

Notes on request body
- The controller expects a single string from the request body. The JSON body must therefore be the quoted sentence (for example: "I want a gift").
- If you prefer to test via Swagger UI, provide the sentence as a JSON string when calling the POST method.

---

## Example response

On success the API returns the `Recommendations` DTO. `Recommendations.Items` is a list where every element is a dictionary mapping dataset column names to string values (the original dataset row values). Example response (trimmed / illustrative):

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "items": [
    {
      "title": "Acrylic Paint Set for Beginners",
      "description": "A 24-color acrylic paint set with brushes and palette.",
      "price": "19.99",
      "ratings": "4.6",
      "no_of_ratings": "324",
      "main_category": "ArtsCrafts"
    },
    {
      "title": "Portable Easel Stand",
      "description": "Lightweight adjustable easel for studio or travel.",
      "price": "34.50",
      "ratings": "4.4",
      "no_of_ratings": "128",
      "main_category": "ArtsCrafts"
    },
    {
      "title": "Watercolor Brush Set",
      "description": "Set of 15 synthetic brushes suitable for watercolors and acrylics.",
      "price": "12.00",
      "ratings": "4.3",
      "no_of_ratings": "210",
      "main_category": "ArtsCrafts"
    }
  ]
}
```

Note: actual returned keys depend on the dataset header (the code converts each row to a dictionary using the header columns). The example above shows typical column names.

---

## Errors and common issues

- "Crazy input!": thrown when the input sentence is too short (< 3 tokens) or a verb cannot be located. Provide a longer sentence and ensure it contains a verb.
- Missing `data/products.csv.gz` or lexicon files: the service will fail during startup or when the recommender runs. Ensure the file paths exist and are readable by the running process.
- Authentication failures: ensure `API_KEY` environment variable is set and the header `MY-API-KEY` in your request matches its value.

---

## Limitations and notes

- The recommender uses an in-memory naive Bayes and lightweight matrix factorization. It is intended as an educational/demo system and may be slow or nondeterministic for very large datasets.
- Matrix factorization uses randomized initialization; returned recommendations may vary across calls.
- The system expects the dataset to be preprocessed into a tab-separated file with a header matching the column names referenced at runtime.

---

## Contributing / Extending

- Replace or improve the dataset loader to support larger datasets or incremental loading.
- Replace the MatrixFactorization implementation with a production-ready recommender or use a persisted pre-trained model.
- Add stronger input validation and richer API models (e.g., accept a JSON object with additional parameters rather than a raw string).

---

If you need a customized example request for a particular target or help wiring the dataset, mention the dataset header and I can add a tailored example request/response.
