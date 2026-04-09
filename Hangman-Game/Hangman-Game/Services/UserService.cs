using Hangman_Game.Helpers;
using Hangman_Game.Models;
using Hangman_Game.Services.Interfaces;
using System.IO;
using System.Text.Json;

namespace Hangman_Game.Services;

public class UserService : IUserService
{
    private readonly string _dataFolder;
    private readonly string _defaultAvatarsFolder;
    private readonly string _customAvatarsFolder;
    private readonly string _usersFile;

    public UserService()
    {
        _dataFolder = PathHelper.EnsureDirectory("Data");
        _defaultAvatarsFolder = PathHelper.EnsureDirectory(Path.Combine("Assets", "Avatars", "Default"));
        _customAvatarsFolder = PathHelper.EnsureDirectory(Path.Combine("Assets", "Avatars", "Custom"));
        _usersFile = PathHelper.EnsureFileExists(Path.Combine("Data", "users.json"), "[]");
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
            throw new InvalidOperationException("A user with this name already exists.");

        users.Add(user);
        SaveAll(users);
    }

    public void DeleteUser(string username)
    {
        var users = GetAllUsers();

        var userToRemove = users.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (userToRemove == null)
            return;

        string avatarPathToCheck = userToRemove.AvatarPath;

        users.Remove(userToRemove);
        SaveAll(users);

        DeleteAvatarIfUnused(avatarPathToCheck, users);
    }

    public bool UserExists(string username)
    {
        return GetAllUsers().Any(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public List<string> GetPredefinedAvatars()
    {
        return GetAvatarsFromFolder(_defaultAvatarsFolder);
    }

    public List<string> GetCustomAvatars()
    {
        return GetAvatarsFromFolder(_customAvatarsFolder);
    }

    public List<string> GetAllAvailableAvatars()
    {
        return GetPredefinedAvatars()
            .Concat(GetCustomAvatars())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path)
            .ToList();
    }

    private List<string> GetAvatarsFromFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            return new List<string>();

        var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif" };

        return Directory.GetFiles(folderPath)
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

    private void DeleteAvatarIfUnused(string avatarRelativePath, List<User> remainingUsers)
    {
        if (string.IsNullOrWhiteSpace(avatarRelativePath))
            return;

        bool avatarStillUsed = remainingUsers.Any(u =>
            u.AvatarPath.Equals(avatarRelativePath, StringComparison.OrdinalIgnoreCase));

        if (avatarStillUsed)
            return;

        string fullAvatarPath = PathHelper.ToAbsolutePath(avatarRelativePath);
        string normalizedAvatarPath = Path.GetFullPath(fullAvatarPath);
        string fullCustomAvatarsFolder = Path.GetFullPath(_customAvatarsFolder);

        bool isInsideCustomFolder = normalizedAvatarPath.StartsWith(
            fullCustomAvatarsFolder,
            StringComparison.OrdinalIgnoreCase);

        if (!isInsideCustomFolder)
            return;

        if (!File.Exists(normalizedAvatarPath))
            return;

        try
        {
            File.Delete(normalizedAvatarPath);
        }
        catch
        {
        }
    }
}