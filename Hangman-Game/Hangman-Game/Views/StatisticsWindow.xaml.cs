using Hangman_Game.Services;
using Hangman_Game.ViewModels;
using System.Windows;

namespace Hangman_Game.Views;

public partial class StatisticsWindow : Window
{
    #region Fields

    private readonly StatisticsVM _viewModel;

    #endregion

    #region Constructors

    public StatisticsWindow()
    {
        InitializeComponent();

        _viewModel = new StatisticsVM(new StatisticsService());
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