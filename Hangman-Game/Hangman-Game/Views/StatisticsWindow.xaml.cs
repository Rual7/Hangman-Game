using Hangman_Game.Services.Interfaces;
using Hangman_Game.ViewModels;
using System.Windows;

namespace Hangman_Game.Views;

public partial class StatisticsWindow : Window
{
    #region Fields

    private readonly StatisticsVM _viewModel;

    #endregion

    #region Constructors

    public StatisticsWindow(IStatisticsService statisticsService)
    {
        InitializeComponent();

        _viewModel = new StatisticsVM(statisticsService);
        DataContext = _viewModel;

        _viewModel.CloseRequested += OnCloseRequested;
    }

    #endregion

    #region Private Event Handlers

    private void OnCloseRequested()
    {
        Close();
    }

    #endregion
}