using Hangman_Game.Helpers;
using Hangman_Game.Models;
using Hangman_Game.Services.Interfaces;
using System.IO;
using System.Xml.Linq;

namespace Hangman_Game.Services;

public class GameService : IGameService
{
    private readonly string _wordsFilePath;
    private readonly Random _random = new();

    public GameService()
    {
        _wordsFilePath = PathHelper.ToAbsolutePath(Path.Combine("Data", "words.xml"));
    }

    public List<string> GetCategories()
    {
        var doc = XDocument.Load(_wordsFilePath);

        var categories = doc.Root!
            .Elements("Category")
            .Select(c => c.Attribute("name")?.Value)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .ToList();

        categories.Insert(0, "All categories");

        return categories;
    }

    public GameSession StartNewGame(string username, string category)
    {
        var normalizedCategory = NormalizeCategory(category);
        var wordEntry = GetRandomWordEntry(normalizedCategory);

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

    public bool GuessLetter(GameSession session, char letter)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (string.IsNullOrWhiteSpace(session.WordToGuess))
            throw new InvalidOperationException("There is no active word in the current session.");

        letter = char.ToLower(letter);

        if (!char.IsLetter(letter))
            return false;

        if (session.GuessedLetters.Contains(letter) || session.WrongLetters.Contains(letter))
            return false;

        if (session.WordToGuess.Contains(letter))
        {
            session.GuessedLetters.Add(letter);
            return true;
        }

        session.WrongLetters.Add(letter);
        session.WrongGuessesCount++;

        return false;
    }

    public string GetMaskedWord(GameSession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (string.IsNullOrWhiteSpace(session.WordToGuess))
            return string.Empty;

        return string.Join(" ",
            session.WordToGuess.Select(c =>
                session.GuessedLetters.Contains(char.ToLower(c)) ? c : '_'));
    }

    public bool IsLevelWon(GameSession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (string.IsNullOrWhiteSpace(session.WordToGuess))
            return false;

        return session.WordToGuess.All(c => session.GuessedLetters.Contains(char.ToLower(c)));
    }

    public bool IsLevelLost(GameSession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        return session.WrongGuessesCount >= session.MaxWrongGuesses
               || session.RemainingSeconds <= 0;
    }

    public bool IsGameWon(GameSession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        return session.ConsecutiveWins >= 3;
    }

    public void StartNextLevel(GameSession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (!IsLevelWon(session))
            throw new InvalidOperationException("You cannot proceed to the next level before winning the current level.");

        session.ConsecutiveWins++;

        if (IsGameWon(session))
        {
            session.CurrentLevel = 3;
            return;
        }

        session.CurrentLevel = session.ConsecutiveWins + 1;

        var wordEntry = GetRandomWordEntry(session.Category);
        session.Category = wordEntry.Category;
        session.WordToGuess = wordEntry.Word;

        session.GuessedLetters.Clear();
        session.WrongLetters.Clear();
        session.WrongGuessesCount = 0;
        session.RemainingSeconds = 300;
    }

    public void ResetProgress(GameSession session, string category)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        var normalizedCategory = NormalizeCategory(category);
        var wordEntry = GetRandomWordEntry(normalizedCategory);

        session.Category = wordEntry.Category;
        session.WordToGuess = wordEntry.Word;

        session.GuessedLetters.Clear();
        session.WrongLetters.Clear();
        session.WrongGuessesCount = 0;

        session.CurrentLevel = 1;
        session.ConsecutiveWins = 0;

        session.RemainingSeconds = 300;
    }

    private (string Word, string Category) GetRandomWordEntry(string category)
    {
        var doc = XDocument.Load(_wordsFilePath);
        var categories = doc.Root!.Elements("Category").ToList();

        if (category.Equals("All categories", StringComparison.OrdinalIgnoreCase))
        {
            var allWords = categories
                .SelectMany(c => c.Elements("Word")
                    .Select(w => new
                    {
                        Category = c.Attribute("name")?.Value?.Trim() ?? "Unknown",
                        Word = w.Value.Trim().ToLower()
                    }))
                .Where(x => !string.IsNullOrWhiteSpace(x.Word))
                .ToList();

            if (allWords.Count == 0)
                throw new InvalidOperationException("There are no words available in words.xml.");

            var chosen = allWords[_random.Next(allWords.Count)];
            return (chosen.Word, chosen.Category);
        }

        var selectedCategory = categories.FirstOrDefault(c =>
            string.Equals(c.Attribute("name")?.Value, category, StringComparison.OrdinalIgnoreCase));

        if (selectedCategory == null)
            throw new InvalidOperationException($"Category '{category}' does not exist.");

        var words = selectedCategory
            .Elements("Word")
            .Select(w => w.Value.Trim().ToLower())
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .ToList();

        if (words.Count == 0)
            throw new InvalidOperationException($"Category '{category}' does not contain any words.");

        return (words[_random.Next(words.Count)], selectedCategory.Attribute("name")?.Value?.Trim() ?? category);
    }

    private string NormalizeCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return "All categories";

        return category.Trim();
    }
}