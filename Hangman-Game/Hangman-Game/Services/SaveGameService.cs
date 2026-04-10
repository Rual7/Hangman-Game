using Hangman_Game.Helpers;
using Hangman_Game.Models;
using Hangman_Game.Services.Interfaces;
using System.IO;
using System.Text.Json;

namespace Hangman_Game.Services;

public class SaveGameService : ISaveGameService
{
    #region Fields

    private readonly string _savesFolderPath;

    #endregion

    #region Constructors

    public SaveGameService()
    {
        _savesFolderPath = PathHelper.EnsureDirectory(Path.Combine("Data", "Saves"));
    }

    #endregion

    #region Public Save Retrieval Methods

    public List<SavedGame> GetAllSaves(string username)
    {
        string saveFilePath = GetUserSaveFilePath(username);

        if (!File.Exists(saveFilePath))
        {
            return new List<SavedGame>();
        }

        string json = File.ReadAllText(saveFilePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<SavedGame>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<SavedGame>>(json) ?? new List<SavedGame>();
        }
        catch
        {
            return new List<SavedGame>();
        }
    }

    public SavedGame? LoadGame(string username, string saveName)
    {
        List<SavedGame> saves = GetAllSaves(username);

        return saves.FirstOrDefault(save =>
            save.SaveName.Equals(saveName, StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Public Save Management Methods

    public void SaveGame(SavedGame save)
    {
        if (save == null)
        {
            throw new ArgumentNullException(nameof(save));
        }

        if (string.IsNullOrWhiteSpace(save.Username))
        {
            throw new InvalidOperationException("Username can't be empty.");
        }

        if (string.IsNullOrWhiteSpace(save.SaveName))
        {
            throw new InvalidOperationException("SaveName can't be empty.");
        }

        List<SavedGame> saves = GetAllSaves(save.Username);

        SavedGame? existingSave = saves.FirstOrDefault(existing =>
            existing.SaveName.Equals(save.SaveName, StringComparison.OrdinalIgnoreCase));

        save.SavedAt = DateTime.Now;

        if (existingSave != null)
        {
            int existingIndex = saves.IndexOf(existingSave);
            saves[existingIndex] = save;
        }
        else
        {
            saves.Add(save);
        }

        SaveAll(save.Username, saves);
    }

    public void DeleteSave(string username, string saveName)
    {
        List<SavedGame> saves = GetAllSaves(username);

        SavedGame? saveToRemove = saves.FirstOrDefault(save =>
            save.SaveName.Equals(saveName, StringComparison.OrdinalIgnoreCase));

        if (saveToRemove == null)
        {
            return;
        }

        saves.Remove(saveToRemove);

        if (saves.Count == 0)
        {
            DeleteAllSaves(username);
            return;
        }

        SaveAll(username, saves);
    }

    public void DeleteAllSaves(string username)
    {
        string saveFilePath = GetUserSaveFilePath(username);

        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
        }
    }

    #endregion

    #region Private Persistence Methods

    private void SaveAll(string username, List<SavedGame> saves)
    {
        string saveFilePath = GetUserSaveFilePath(username);

        JsonSerializerOptions serializerOptions = new()
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(saves, serializerOptions);
        File.WriteAllText(saveFilePath, json);
    }

    #endregion

    #region Private Helper Methods

    private string GetUserSaveFilePath(string username)
    {
        string safeUsername = username.Trim();
        return Path.Combine(_savesFolderPath, $"{safeUsername}_saves.json");
    }

    #endregion
}