using Hangman_Game.Models;

namespace Hangman_Game.Services.Interfaces;

public interface IUserService
{
    #region User Management

    List<User> GetAllUsers();

    void AddUser(User user);

    void DeleteUser(string username);

    bool UserExists(string username);

    #endregion

    #region Avatar Retrieval

    List<string> GetPredefinedAvatars();

    List<string> GetCustomAvatars();

    List<string> GetAllAvailableAvatars();

    #endregion
}