using Hangman_Game.Helpers;
using Hangman_Game.Models;
using Hangman_Game.Services.Interfaces;
using System.IO;
using System.Xml.Linq;

namespace Hangman_Game.Services;

public class GameService : IGameService
{
    #region Fields

    private readonly string _wordsFilePath;
    private readonly Random _random = new();

    #endregion

    #region Constructors

    public GameService()
    {
        _wordsFilePath = PathHelper.ToAbsolutePath(Path.Combine("Data", "words.xml"));
    }

    #endregion

    #region Public Game Setup Methods

    public List<string> GetCategories()
    {
        XDocument wordsDocument = LoadWordsDocument();

        List<string> categories = wordsDocument.Root!
            .Elements("Category")
            .Select(categoryElement => categoryElement.Attribute("name")?.Value)
            .Where(categoryName => !string.IsNullOrWhiteSpace(categoryName))
            .Cast<string>()
            .ToList();

        categories.Insert(0, "All categories");

        return categories;
    }

    public GameSession StartNewGame(string username, string category)
    {
        string normalizedCategory = NormalizeCategory(category);
        (string Word, string Category) wordEntry = GetRandomWordEntry(normalizedCategory);

        return new GameSession
        {
            Username = username,
            Category = wordEntry.Category,
            WordToGuess = wordEntry.Word,
            GuessedLetters = new HashSet<char>(),
            WrongLetters = new HashSet<char>(),
            WrongGuessesCount = 0,
            MaxWrongGuesses = 6,
            CurrentLevel = 1,
            ConsecutiveWins = 0,
            RemainingSeconds = 300
        };
    }

    public void ResetProgress(GameSession session, string category)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        string normalizedCategory = NormalizeCategory(category);
        (string Word, string Category) wordEntry = GetRandomWordEntry(normalizedCategory);

        session.Category = wordEntry.Category;
        session.WordToGuess = wordEntry.Word;
        session.GuessedLetters.Clear();
        session.WrongLetters.Clear();
        session.WrongGuessesCount = 0;
        session.CurrentLevel = 1;
        session.ConsecutiveWins = 0;
        session.RemainingSeconds = 300;
    }

    #endregion

    #region Public Gameplay Methods

    public bool GuessLetter(GameSession session, char letter)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        if (string.IsNullOrWhiteSpace(session.WordToGuess))
        {
            throw new InvalidOperationException("There is no active word in the current session.");
        }

        char normalizedLetter = char.ToLower(letter);

        if (!char.IsLetter(normalizedLetter))
        {
            return false;
        }

        if (session.GuessedLetters.Contains(normalizedLetter) || session.WrongLetters.Contains(normalizedLetter))
        {
            return false;
        }

        if (session.WordToGuess.Contains(normalizedLetter))
        {
            session.GuessedLetters.Add(normalizedLetter);
            return true;
        }

        session.WrongLetters.Add(normalizedLetter);
        session.WrongGuessesCount++;

        return false;
    }

    public string GetMaskedWord(GameSession session)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        if (string.IsNullOrWhiteSpace(session.WordToGuess))
        {
            return string.Empty;
        }

        return string.Join(" ",
            session.WordToGuess.Select(character =>
                session.GuessedLetters.Contains(char.ToLower(character)) ? character : '_'));
    }

    #endregion

    #region Public Progress Evaluation Methods

    public bool IsLevelWon(GameSession session)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        if (string.IsNullOrWhiteSpace(session.WordToGuess))
        {
            return false;
        }

        return session.WordToGuess.All(character =>
            session.GuessedLetters.Contains(char.ToLower(character)));
    }

    public bool IsLevelLost(GameSession session)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        return session.WrongGuessesCount >= session.MaxWrongGuesses
               || session.RemainingSeconds <= 0;
    }

    public bool IsGameWon(GameSession session)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        return session.ConsecutiveWins >= 3;
    }

    public void StartNextLevel(GameSession session)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        if (!IsLevelWon(session))
        {
            throw new InvalidOperationException("You cannot proceed to the next level before winning the current level.");
        }

        session.ConsecutiveWins++;

        if (IsGameWon(session))
        {
            session.CurrentLevel = 3;
            return;
        }

        session.CurrentLevel = session.ConsecutiveWins + 1;

        (string Word, string Category) wordEntry = GetRandomWordEntry(session.Category);

        session.Category = wordEntry.Category;
        session.WordToGuess = wordEntry.Word;
        session.GuessedLetters.Clear();
        session.WrongLetters.Clear();
        session.WrongGuessesCount = 0;
        session.RemainingSeconds = 300;
    }

    #endregion

    #region Private Word Loading Methods

    private XDocument LoadWordsDocument()
    {
        return XDocument.Load(_wordsFilePath);
    }

    private (string Word, string Category) GetRandomWordEntry(string category)
    {
        XDocument wordsDocument = LoadWordsDocument();
        List<XElement> categories = wordsDocument.Root!.Elements("Category").ToList();

        if (category.Equals("All categories", StringComparison.OrdinalIgnoreCase))
        {
            var allWords = categories
                .SelectMany(categoryElement => categoryElement.Elements("Word")
                    .Select(wordElement => new
                    {
                        Category = categoryElement.Attribute("name")?.Value?.Trim() ?? "Unknown",
                        Word = wordElement.Value.Trim().ToLower()
                    }))
                .Where(wordEntry => !string.IsNullOrWhiteSpace(wordEntry.Word))
                .ToList();

            if (allWords.Count == 0)
            {
                throw new InvalidOperationException("There are no words available in words.xml.");
            }

            var chosenWord = allWords[_random.Next(allWords.Count)];
            return (chosenWord.Word, chosenWord.Category);
        }

        XElement? selectedCategory = categories.FirstOrDefault(categoryElement =>
            string.Equals(categoryElement.Attribute("name")?.Value, category, StringComparison.OrdinalIgnoreCase));

        if (selectedCategory == null)
        {
            throw new InvalidOperationException($"Category '{category}' does not exist.");
        }

        List<string> words = selectedCategory
            .Elements("Word")
            .Select(wordElement => wordElement.Value.Trim().ToLower())
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .ToList();

        if (words.Count == 0)
        {
            throw new InvalidOperationException($"Category '{category}' does not contain any words.");
        }

        return (
            words[_random.Next(words.Count)],
            selectedCategory.Attribute("name")?.Value?.Trim() ?? category);
    }

    #endregion

    #region Private Helper Methods

    private string NormalizeCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return "All categories";
        }

        return category.Trim();
    }

    #endregion
}