using Hangman_Game.Helpers;
using Hangman_Game.Models;
using Hangman_Game.Services.Interfaces;
using System.IO;
using System.Text.Json;

namespace Hangman_Game.Services;

public class StatisticsService : IStatisticsService
{
    private readonly string _statisticsFile;

    public StatisticsService()
    {
        string dataFolder = PathHelper.EnsureDirectory("Data");
        _statisticsFile = Path.Combine(dataFolder, "statistics.json");

        EnsureStatisticsFileExists();
    }

    public List<UserCategoryStatistic> GetAllStatistics()
    {
        if (!File.Exists(_statisticsFile))
            return new List<UserCategoryStatistic>();

        string json = File.ReadAllText(_statisticsFile);

        if (string.IsNullOrWhiteSpace(json))
            return new List<UserCategoryStatistic>();

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

    public void RegisterGamePlayed(string username, string category, bool won)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(category))
            return;

        var statistics = GetAllStatistics();

        var existingEntry = statistics.FirstOrDefault(s =>
            s.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
            s.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

        if (existingEntry == null)
        {
            existingEntry = new UserCategoryStatistic
            {
                Username = username.Trim(),
                Category = category.Trim(),
                GamesPlayed = 0,
                GamesWon = 0
            };

            statistics.Add(existingEntry);
        }

        existingEntry.GamesPlayed++;

        if (won)
            existingEntry.GamesWon++;

        SaveAll(statistics);
    }

    public void DeleteUserStatistics(string username)
    {
        var statistics = GetAllStatistics();

        statistics = statistics
            .Where(s => !s.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
            .ToList();

        SaveAll(statistics);
    }

    private void SaveAll(List<UserCategoryStatistic> statistics)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(statistics, options);
        File.WriteAllText(_statisticsFile, json);
    }

    private void EnsureStatisticsFileExists()
    {
        if (!File.Exists(_statisticsFile))
        {
            File.WriteAllText(_statisticsFile, "[]");
        }
    }
}