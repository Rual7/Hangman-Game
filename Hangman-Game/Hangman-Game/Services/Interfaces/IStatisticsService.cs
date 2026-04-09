using Hangman_Game.Models;

namespace Hangman_Game.Services.Interfaces;

public interface IStatisticsService
{
    List<UserCategoryStatistic> GetAllStatistics();
    void RegisterGamePlayed(string username, string category, bool won);
    void DeleteUserStatistics(string username);
}
