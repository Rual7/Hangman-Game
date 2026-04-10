using Hangman_Game.Helpers;
using Hangman_Game.Models;
using Hangman_Game.Services.Interfaces;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace Hangman_Game.Services;

public class UserService : IUserService
{
    #region Fields

    private readonly string _defaultAvatarsFolderPath;
    private readonly string _customAvatarsFolderPath;
    private readonly string _usersFilePath;

    #endregion

    #region Constructors

    public UserService()
    {
        PathHelper.EnsureDirectory("Data");
        _defaultAvatarsFolderPath = PathHelper.EnsureDirectory(Path.Combine("Assets", "Avatars", "Default"));
        _customAvatarsFolderPath = PathHelper.EnsureDirectory(Path.Combine("Assets", "Avatars", "Custom"));
        _usersFilePath = PathHelper.EnsureFileExists(Path.Combine("Data", "users.json"), "[]");
    }

    #endregion

    #region Public User Management Methods

    public List<User> GetAllUsers()
    {
        if (!File.Exists(_usersFilePath))
        {
            return new List<User>();
        }

        string json = File.ReadAllText(_usersFilePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<User>();
        }

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
        List<User> users = GetAllUsers();

        if (users.Any(existingUser =>
                existingUser.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("A user with this name already exists.");
        }

        users.Add(user);
        SaveAll(users);
    }

    public void DeleteUser(string username)
    {
        List<User> users = GetAllUsers();

        User? userToRemove = users.FirstOrDefault(existingUser =>
            existingUser.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (userToRemove == null)
        {
            return;
        }

        string avatarRelativePath = userToRemove.AvatarPath;

        users.Remove(userToRemove);
        SaveAll(users);

        DeleteAvatarIfUnused(avatarRelativePath, users);
    }

    public bool UserExists(string username)
    {
        return GetAllUsers().Any(existingUser =>
            existingUser.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Public Avatar Retrieval Methods

    public List<string> GetPredefinedAvatars()
    {
        return GetAvatarsFromFolder(_defaultAvatarsFolderPath);
    }

    public List<string> GetCustomAvatars()
    {
        return GetAvatarsFromFolder(_customAvatarsFolderPath);
    }

    public List<string> GetAllAvailableAvatars()
    {
        return GetPredefinedAvatars()
            .Concat(GetCustomAvatars())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path)
            .ToList();
    }

    #endregion

    #region Private Persistence Methods

    private void SaveAll(List<User> users)
    {
        JsonSerializerOptions serializerOptions = new()
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(users, serializerOptions);
        File.WriteAllText(_usersFilePath, json);
    }

    #endregion

    #region Private Avatar Methods

    private List<string> GetAvatarsFromFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            return new List<string>();
        }

        string[] allowedExtensions = { ".png", ".jpg", ".jpeg", ".gif" };

        return Directory.GetFiles(folderPath)
            .Where(filePath =>
                allowedExtensions.Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase))
            .Select(PathHelper.ToRelativePath)
            .OrderBy(path => path)
            .ToList();
    }

    private void DeleteAvatarIfUnused(string avatarPath, List<User> remainingUsers)
    {
        if (string.IsNullOrWhiteSpace(avatarPath))
        {
            return;
        }

        bool avatarStillUsed = remainingUsers.Any(user =>
            user.AvatarPath.Equals(avatarPath, StringComparison.OrdinalIgnoreCase));

        if (avatarStillUsed)
        {
            return;
        }

        string fullPath = Path.IsPathRooted(avatarPath)
            ? Path.GetFullPath(avatarPath)
            : Path.GetFullPath(PathHelper.ToAbsolutePath(avatarPath));

        string customFolderFullPath = Path.GetFullPath(_customAvatarsFolderPath);

        if (!fullPath.StartsWith(customFolderFullPath + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!File.Exists(fullPath))
        {
            return;
        }

        try
        {
            File.Delete(fullPath);
        }
        catch
        {
        }
    }

    #endregion
}