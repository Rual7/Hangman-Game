using Hangman_Game.Models;

namespace Hangman_Game.Services.Interfaces;

public interface IUserService
{
    List<User> GetAllUsers();
    void AddUser(User user);
    void DeleteUser(string username);
    bool UserExists(string username);

    List<string> GetPredefinedAvatars();
    List<string> GetCustomAvatars();
    List<string> GetAllAvailableAvatars();
}
