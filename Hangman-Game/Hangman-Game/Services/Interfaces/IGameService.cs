using Hangman_Game.Models;

namespace Hangman_Game.Services.Interfaces;

public interface IGameService
{
    #region Game Setup

    List<string> GetCategories();

    GameSession StartNewGame(string username, string category);

    void ResetProgress(GameSession session, string category);

    #endregion

    #region Gameplay

    bool GuessLetter(GameSession session, char letter);

    string GetMaskedWord(GameSession session);

    #endregion

    #region Progress Evaluation

    bool IsLevelWon(GameSession session);

    bool IsLevelLost(GameSession session);

    bool IsGameWon(GameSession session);

    void StartNextLevel(GameSession session);

    #endregion
}