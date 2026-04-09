using Hangman_Game.Models;

namespace Hangman_Game.Services.Interfaces;

public interface ISaveGameService
{
    List<SavedGame> GetAllSaves(string username);
    void SaveGame(SavedGame save);
    SavedGame? LoadGame(string username, string saveName);
    void DeleteSave(string username, string saveName);
    void DeleteAllSaves(string username);
}
