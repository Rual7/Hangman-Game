using Hangman_Game.Models;
using Hangman_Game.Services.Interfaces;
using Hangman_Game.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Hangman_Game.ViewModels;

public class StatisticsVM : BaseVM
{
    #region Fields

    private readonly IStatisticsService _statisticsService;

    #endregion

    #region Collections

    public ObservableCollection<UserCategoryStatistic> Statistics { get; } = new();

    #endregion

    #region Bindable Properties

    public bool HasStatistics => Statistics.Count > 0;

    #endregion

    #region Commands

    public ICommand CloseCommand { get; }

    #endregion

    #region Events

    public event Action? CloseRequested;

    #endregion

    #region Constructors

    public StatisticsVM(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;

        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke());

        LoadStatistics();
    }

    #endregion

    #region Public Methods

    public void LoadStatistics()
    {
        Statistics.Clear();

        foreach (UserCategoryStatistic statistic in _statisticsService
                     .GetAllStatistics()
                     .OrderBy(statistic => statistic.Username)
                     .ThenBy(statistic => statistic.Category))
        {
            Statistics.Add(statistic);
        }

        OnPropertyChanged(nameof(HasStatistics));
    }

    #endregion
}