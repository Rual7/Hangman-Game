namespace Hangman_Game.Models;

public class GameSession
{
    #region Player and Word Data

    public string Username { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string WordToGuess { get; set; } = string.Empty;

    #endregion

    #region Letter State

    public HashSet<char> GuessedLetters { get; set; } = new();

    public HashSet<char> WrongLetters { get; set; } = new();

    #endregion

    #region Game Progress

    public int WrongGuessesCount { get; set; } = 0;

    public int MaxWrongGuesses { get; set; } = 6;

    public int CurrentLevel { get; set; } = 1;

    public int ConsecutiveWins { get; set; } = 0;

    public int RemainingSeconds { get; set; } = 300;

    #endregion
}