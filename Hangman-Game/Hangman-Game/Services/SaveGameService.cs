using Hangman_Game.Helpers;
using Hangman_Game.Models;
using Hangman_Game.Services.Interfaces;
using System.IO;
using System.Text.Json;

namespace Hangman_Game.Services;

public class SaveGameService : ISaveGameService
{
    private readonly string _savesFolder;

    public SaveGameService()
    {
        _savesFolder = PathHelper.EnsureDirectory(Path.Combine("Data", "Saves"));
    }

    public List<SavedGame> GetAllSaves(string username)
    {
        string saveFile = GetUserSaveFile(username);

        if (!File.Exists(saveFile))
            return new List<SavedGame>();

        string json = File.ReadAllText(saveFile);

        if (string.IsNullOrWhiteSpace(json))
            return new List<SavedGame>();

        try
        {
            return JsonSerializer.Deserialize<List<SavedGame>>(json) ?? new List<SavedGame>();
        }
        catch
        {
            return new List<SavedGame>();
        }
    }

    public void SaveGame(SavedGame save)
    {
        if (save == null)
            throw new ArgumentNullException(nameof(save));

        if (string.IsNullOrWhiteSpace(save.Username))
            throw new InvalidOperationException("Username can't be empty.");

        if (string.IsNullOrWhiteSpace(save.SaveName))
            throw new InvalidOperationException("SaveName can't be empty.");

        var saves = GetAllSaves(save.Username);

        var existingSave = saves.FirstOrDefault(s =>
            s.SaveName.Equals(save.SaveName, StringComparison.OrdinalIgnoreCase));

        save.SavedAt = DateTime.Now;

        if (existingSave != null)
        {
            int index = saves.IndexOf(existingSave);
            saves[index] = save;
        }
        else
        {
            saves.Add(save);
        }

        SaveAll(save.Username, saves);
    }

    public SavedGame? LoadGame(string username, string saveName)
    {
        var saves = GetAllSaves(username);

        return saves.FirstOrDefault(s =>
            s.SaveName.Equals(saveName, StringComparison.OrdinalIgnoreCase));
    }

    public void DeleteSave(string username, string saveName)
    {
        var saves = GetAllSaves(username);

        var saveToRemove = saves.FirstOrDefault(s =>
            s.SaveName.Equals(saveName, StringComparison.OrdinalIgnoreCase));

        if (saveToRemove == null)
            return;

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
        string saveFile = GetUserSaveFile(username);

        if (File.Exists(saveFile))
        {
            File.Delete(saveFile);
        }
    }

    private void SaveAll(string username, List<SavedGame> saves)
    {
        string saveFile = GetUserSaveFile(username);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(saves, options);
        File.WriteAllText(saveFile, json);
    }

    private string GetUserSaveFile(string username)
    {
        string safeUsername = username.Trim();
        return Path.Combine(_savesFolder, $"{safeUsername}_saves.json");
    }
}