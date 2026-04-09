using Hangman_Game.Models;

namespace Hangman_Game.Services.Interfaces;

public interface IGameService
{
    List<string> GetCategories();

    GameSession StartNewGame(string username, string category);

    bool GuessLetter(GameSession session, char letter);

    string GetMaskedWord(GameSession session);

    bool IsLevelWon(GameSession session);

    bool IsLevelLost(GameSession session);

    bool IsGameWon(GameSession session);

    void StartNextLevel(GameSession session);

    void ResetProgress(GameSession session, string category);
}
