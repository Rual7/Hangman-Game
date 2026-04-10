using Hangman_Game.Models;
using Hangman_Game.Services.Interfaces;
using Hangman_Game.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace Hangman_Game.Views;

public partial class GameWindow : Window
{
    #region Fields

    private readonly GameVM _viewModel;

    #endregion

    #region Constructors

    public GameWindow(
        User user,
        IGameService gameService,
        ISaveGameService saveGameService,
        IStatisticsService statisticsService,
        IUserService userService)
    {
        InitializeComponent();

        _viewModel = new GameVM(
            user,
            gameService,
            saveGameService,
            statisticsService,
            userService);

        DataContext = _viewModel;

        SubscribeToViewModelEvents();
        Closing += OnWindowClosing;
    }

    #endregion

    #region Private Event Subscriptions

    private void SubscribeToViewModelEvents()
    {
        _viewModel.CancelRequested += OnCancelRequested;
        _viewModel.StatisticsRequested += OnStatisticsRequested;
        _viewModel.AboutRequested += OnAboutRequested;
        _viewModel.SaveNameRequested += OnSaveNameRequested;
        _viewModel.ConfirmCloseRequested += OnConfirmCloseRequested;
    }

    #endregion

    #region Private Event Handlers

    private void OnCancelRequested()
    {
        Close();
    }

    private void OnStatisticsRequested()
    {
        if (Owner is StartWindow startWindow)
        {
            StatisticsWindow statisticsWindow = new(
                GetStatisticsService(startWindow))
            {
                Owner = this
            };

            statisticsWindow.ShowDialog();
        }
    }

    private void OnAboutRequested()
    {
        MessageBox.Show(
            "Hangman-Game\n\nStudent: Oncioiu Ionut-Raul\nGroup: 10LF243\nEmail: ionut.oncioiu@student.unitbv.ro",
            "About",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private string? OnSaveNameRequested()
    {
        InputDialog inputDialog = new("Enter a name for the save:")
        {
            Owner = this
        };

        bool? result = inputDialog.ShowDialog();

        if (result != true || string.IsNullOrWhiteSpace(inputDialog.InputText))
        {
            return null;
        }

        return inputDialog.InputText.Trim();
    }

    private bool OnConfirmCloseRequested()
    {
        MessageBoxResult result = MessageBox.Show(
            "You have a game in progress. Do you want to save it before exiting?",
            "Save Game",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Cancel)
        {
            return false;
        }

        if (result == MessageBoxResult.Yes)
        {
            _viewModel.Menu.SaveGameCommand.Execute(null);
        }

        return true;
    }

    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        if (!_viewModel.ConfirmWindowClose())
        {
            e.Cancel = true;
        }
    }

    #endregion

    #region Private Helper Methods

    private static IStatisticsService GetStatisticsService(StartWindow startWindow)
    {
        return startWindow.StatisticsService;
    }

    #endregion
}