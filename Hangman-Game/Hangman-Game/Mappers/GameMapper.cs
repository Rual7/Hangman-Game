using Hangman_Game.Models;

namespace Hangman_Game.Mappers;

public static class GameMapper
{
    #region Public Mapping Methods

    public static SavedGame ToSavedGame(GameSession gameSession, string saveName)
    {
        return new SavedGame
        {
            SaveName = saveName,
            Username = gameSession.Username,
            Category = gameSession.Category,
            WordToGuess = gameSession.WordToGuess,
            GuessedLetters = gameSession.GuessedLetters.ToList(),
            WrongLetters = gameSession.WrongLetters.ToList(),
            WrongGuessesCount = gameSession.WrongGuessesCount,
            CurrentLevel = gameSession.CurrentLevel,
            ConsecutiveWins = gameSession.ConsecutiveWins,
            RemainingSeconds = gameSession.RemainingSeconds,
            SavedAt = DateTime.Now
        };
    }

    public static GameSession ToGameSession(SavedGame savedGame)
    {
        return new GameSession
        {
            Username = savedGame.Username,
            Category = savedGame.Category,
            WordToGuess = savedGame.WordToGuess,
            GuessedLetters = new HashSet<char>(savedGame.GuessedLetters),
            WrongLetters = new HashSet<char>(savedGame.WrongLetters),
            WrongGuessesCount = savedGame.WrongGuessesCount,
            CurrentLevel = savedGame.CurrentLevel,
            ConsecutiveWins = savedGame.ConsecutiveWins,
            RemainingSeconds = savedGame.RemainingSeconds
        };
    }

    #endregion
}