using Hangman_Game.Models;
using Hangman_Game.Services.Interfaces;
using Hangman_Game.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Hangman_Game.ViewModels;

public class StatisticsVM : BaseVM
{
    private readonly IStatisticsService _statisticsService;

    public ObservableCollection<UserCategoryStatistic> Statistics { get; } = new();

    public bool HasStatistics => Statistics.Count > 0;

    public ICommand CloseCommand { get; }

    public event Action? CloseRequested;

    public StatisticsVM(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;

        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke());

        LoadStatistics();
    }

    public void LoadStatistics()
    {
        Statistics.Clear();

        foreach (var item in _statisticsService
                     .GetAllStatistics()
                     .OrderBy(s => s.Username)
                     .ThenBy(s => s.Category))
        {
            Statistics.Add(item);
        }

        OnPropertyChanged(nameof(HasStatistics));
    }
}