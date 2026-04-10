using Hangman_Game.Models;
using Hangman_Game.Services.Interfaces;
using Hangman_Game.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Hangman_Game.Views;

public partial class GameWindow : Window
{
    #region Fields

    private readonly GameVM _viewModel;
    private readonly IStatisticsService _statisticsService;

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

        _statisticsService = statisticsService;

        _viewModel = new GameVM(
            user,
            gameService,
            saveGameService,
            statisticsService,
            userService);

        DataContext = _viewModel;

        SubscribeToViewModelEvents();
        Closing += OnWindowClosing;
        PreviewKeyDown += OnPreviewKeyDown;
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
        StatisticsWindow statisticsWindow = new(_statisticsService)
        {
            Owner = this
        };

        statisticsWindow.ShowDialog();
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

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!CanHandleKeyboardGuess(e))
        {
            return;
        }

        char? pressedLetter = TryGetLetterFromKey(e.Key);

        if (pressedLetter == null)
        {
            return;
        }

        char upperLetter = char.ToUpperInvariant(pressedLetter.Value);

        if (_viewModel.GuessLetterCommand.CanExecute(upperLetter))
        {
            _viewModel.GuessLetterCommand.Execute(upperLetter);
            e.Handled = true;
        }
    }

    #endregion

    #region Private Helper Methods

    private bool CanHandleKeyboardGuess(KeyEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.None)
        {
            return false;
        }

        if (e.Key == Key.System)
        {
            return false;
        }

        if (Keyboard.FocusedElement is MenuItem)
        {
            return false;
        }

        return true;
    }

    private static char? TryGetLetterFromKey(Key key)
    {
        if (key >= Key.A && key <= Key.Z)
        {
            return (char)('A' + (key - Key.A));
        }

        return null;
    }

    #endregion
}