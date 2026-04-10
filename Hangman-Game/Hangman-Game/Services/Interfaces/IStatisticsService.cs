using Hangman_Game.Models;

namespace Hangman_Game.Services.Interfaces;

public interface IStatisticsService
{
    #region Statistics Retrieval

    List<UserCategoryStatistic> GetAllStatistics();

    #endregion

    #region Statistics Management

    void RegisterGamePlayed(string username, string category, bool won);

    void DeleteUserStatistics(string username);

    #endregion
}