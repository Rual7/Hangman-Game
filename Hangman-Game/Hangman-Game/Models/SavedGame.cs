namespace Hangman_Game.Models;

public class SavedGame
{
    public string SaveName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;
    public string WordToGuess { get; set; } = string.Empty;

    public List<char> GuessedLetters { get; set; } = new();
    public List<char> WrongLetters { get; set; } = new();

    public int WrongGuessesCount { get; set; }
    public int CurrentLevel { get; set; }
    public int ConsecutiveWins { get; set; }

    public int RemainingSeconds { get; set; }

    public DateTime SavedAt { get; set; } = DateTime.Now;
}
