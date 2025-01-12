# Project Overview

This web api application project is designed to generate gift recommendation based on a given sentence. Interested objects are classified into categories in the Amazon Sales 2023 Dataset (Kaggle). Multiple components process different types of data, including verb polarities, synonyms, word roles, and product information. The datasets were generated in a format suitable for use in .NET projects and other analytical tools, then are used to extract contextual information from the sentence and generate a recommendation.

## Techiques Used
- Weighted Naive Bayes
- Matrix Factorization

## Components

### 1. Verb Polarity Dataset
- **Description**: Reads verbs and their polarities from `polarities.txt`.
- **Usage**: Useful for sentiment analysis and understanding verb usage in different contexts.

### 2. Synonyms Dataset
- **Description**: Reads synonyms and their frequencies from `synonyms.txt`. Utilizes the Brown Corpus and Google Ngram Corpus for frequency calculations.
- **Usage**: Provides a comprehensive set of synonyms with frequency data for linguistic research and applications.

### 3. Word Roles Analysis
- **Description**: Analyzes datasets to identify words with multiple roles, providing insights into part-of-speech distribution.
- **Usage**: Helps in understanding the functional versatility of words in language processing.

### 4. Regularized Products Dataset
- **Description**: Processes product data to handle missing prices and ratings, ensuring data completeness and regularity.
- **Usage**: Essential for e-commerce platforms and market analysis applications.

## Technologies Used
- **Programming Languages**: C#
- **Data Sources**: WordNet, Brown Corpus, Google Ngram Corpus, Amazon Sales 2023 Dataset (Kaggle)
