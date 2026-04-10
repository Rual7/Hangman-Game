using Hangman_Game.Models;

namespace Hangman_Game.Services.Interfaces;

public interface ISaveGameService
{
    #region Save Retrieval

    List<SavedGame> GetAllSaves(string username);

    SavedGame? LoadGame(string username, string saveName);

    #endregion

    #region Save Management

    void SaveGame(SavedGame save);

    void DeleteSave(string username, string saveName);

    void DeleteAllSaves(string username);

    #endregion
}