using Hangman_Game.Helpers;
using Hangman_Game.Models;
using Hangman_Game.Services.Interfaces;
using System.IO;
using System.Text.Json;

namespace Hangman_Game.Services;

public class StatisticsService : IStatisticsService
{
    #region Fields

    private readonly string _statisticsFilePath;

    #endregion

    #region Constructors

    public StatisticsService()
    {
        string dataFolderPath = PathHelper.EnsureDirectory("Data");
        _statisticsFilePath = Path.Combine(dataFolderPath, "statistics.json");

        EnsureStatisticsFileExists();
    }

    #endregion

    #region Public Statistics Retrieval Methods

    public List<UserCategoryStatistic> GetAllStatistics()
    {
        if (!File.Exists(_statisticsFilePath))
        {
            return new List<UserCategoryStatistic>();
        }

        string json = File.ReadAllText(_statisticsFilePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<UserCategoryStatistic>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<UserCategoryStatistic>>(json)
                   ?? new List<UserCategoryStatistic>();
        }
        catch
        {
            return new List<UserCategoryStatistic>();
        }
    }

    #endregion

    #region Public Statistics Management Methods

    public void RegisterGamePlayed(string username, string category, bool won)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(category))
        {
            return;
        }

        List<UserCategoryStatistic> statistics = GetAllStatistics();

        UserCategoryStatistic? existingStatistic = statistics.FirstOrDefault(statistic =>
            statistic.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
            statistic.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

        if (existingStatistic == null)
        {
            existingStatistic = new UserCategoryStatistic
            {
                Username = username.Trim(),
                Category = category.Trim(),
                GamesPlayed = 0,
                GamesWon = 0
            };

            statistics.Add(existingStatistic);
        }

        existingStatistic.GamesPlayed++;

        if (won)
        {
            existingStatistic.GamesWon++;
        }

        SaveAll(statistics);
    }

    public void DeleteUserStatistics(string username)
    {
        List<UserCategoryStatistic> statistics = GetAllStatistics();

        statistics = statistics
            .Where(statistic => !statistic.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
            .ToList();

        SaveAll(statistics);
    }

    #endregion

    #region Private Persistence Methods

    private void SaveAll(List<UserCategoryStatistic> statistics)
    {
        JsonSerializerOptions serializerOptions = new()
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(statistics, serializerOptions);
        File.WriteAllText(_statisticsFilePath, json);
    }

    #endregion

    #region Private Initialization Methods

    private void EnsureStatisticsFileExists()
    {
        if (!File.Exists(_statisticsFilePath))
        {
            File.WriteAllText(_statisticsFilePath, "[]");
        }
    }

    #endregion
}