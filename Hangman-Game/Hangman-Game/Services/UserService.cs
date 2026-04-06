using Hangman_Game.Helpers;
using Hangman_Game.Models;
using Hangman_Game.Services.Interfaces;
using System.IO;
using System.Text.Json;

namespace Hangman_Game.Services;

public class UserService : IUserService
{
    private readonly string _dataFolder;
    private readonly string _avatarsFolder;
    private readonly string _usersFile;

    public UserService()
    {
        _dataFolder = PathHelper.EnsureDirectory("Data");
        _avatarsFolder = PathHelper.EnsureDirectory(Path.Combine("Assets", "Avatars"));
        _usersFile = Path.Combine(_dataFolder, "users.json");

        EnsureUsersFileExists();
    }

    public List<User> GetAllUsers()
    {
        if (!File.Exists(_usersFile))
            return new List<User>();

        string json = File.ReadAllText(_usersFile);
        if (string.IsNullOrWhiteSpace(json))
            return new List<User>();

        try
        {
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }
        catch
        {
            return new List<User>();
        }
    }

    public void AddUser(User user)
    {
        var users = GetAllUsers();

        if (users.Any(u => u.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Există deja un utilizator cu acest nume.");

        users.Add(user);
        SaveAll(users);
    }

    public void DeleteUser(string username)
    {
        var users = GetAllUsers();
        var userToRemove = users.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (userToRemove != null)
        {
            users.Remove(userToRemove);
            SaveAll(users);
        }
    }

    public bool UserExists(string username)
    {
        return GetAllUsers().Any(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public List<string> GetPredefinedAvatars()
    {
        if (!Directory.Exists(_avatarsFolder))
            return new List<string>();

        var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif" };

        return Directory.GetFiles(_avatarsFolder)
            .Where(file => allowedExtensions.Contains(Path.GetExtension(file).ToLower()))
            .Select(PathHelper.ToRelativePath)
            .OrderBy(path => path)
            .ToList();
    }

    private void SaveAll(List<User> users)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(users, options);
        File.WriteAllText(_usersFile, json);
    }

    private void EnsureUsersFileExists()
    {
        if (!File.Exists(_usersFile))
        {
            File.WriteAllText(_usersFile, "[]");
        }
    }
}
