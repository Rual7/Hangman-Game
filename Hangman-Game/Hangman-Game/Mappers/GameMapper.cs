using Hangman_Game.Models;

namespace Hangman_Game.Mappers;

public class GameMapper
{
    public static SavedGame ToSavedGame(GameSession session, string saveName)
    {
        return new SavedGame
        {
            SaveName = saveName,
            Username = session.Username,
            Category = session.Category,
            WordToGuess = session.WordToGuess,

            GuessedLetters = session.GuessedLetters.ToList(),
            WrongLetters = session.WrongLetters.ToList(),

            WrongGuessesCount = session.WrongGuessesCount,
            CurrentLevel = session.CurrentLevel,
            ConsecutiveWins = session.ConsecutiveWins,

            RemainingSeconds = session.RemainingSeconds,

            SavedAt = System.DateTime.Now
        };
    }

    public static GameSession ToGameSession(SavedGame saved)
    {
        return new GameSession
        {
            Username = saved.Username,
            Category = saved.Category,
            WordToGuess = saved.WordToGuess,

            GuessedLetters = new HashSet<char>(saved.GuessedLetters),
            WrongLetters = new HashSet<char>(saved.WrongLetters),

            WrongGuessesCount = saved.WrongGuessesCount,
            CurrentLevel = saved.CurrentLevel,
            ConsecutiveWins = saved.ConsecutiveWins,

            RemainingSeconds = saved.RemainingSeconds
        };
    }
}