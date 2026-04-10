using Hangman_Game.Models;
using Hangman_Game.Services.Interfaces;
using Hangman_Game.ViewModels;
using System.Windows;

namespace Hangman_Game.Views;

public partial class StartWindow : Window
{
    #region Fields

    private readonly StartVM _viewModel;
    private readonly IUserService _userService;
    private readonly ISaveGameService _saveGameService;
    private readonly IStatisticsService _statisticsService;
    private readonly IGameService _gameService;

    #endregion

    #region Public Properties

    public IStatisticsService StatisticsService => _statisticsService;

    #endregion

    #region Constructors

    public StartWindow(
        IUserService userService,
        ISaveGameService saveGameService,
        IStatisticsService statisticsService,
        IGameService gameService)
    {
        InitializeComponent();

        _userService = userService;
        _saveGameService = saveGameService;
        _statisticsService = statisticsService;
        _gameService = gameService;

        _viewModel = new StartVM(_userService, _saveGameService, _statisticsService);
        DataContext = _viewModel;

        _viewModel.NewProfileRequested += OnNewProfileRequested;
        _viewModel.DeleteUserRequested += OnDeleteUserRequested;
        _viewModel.PlayRequested += OnPlayRequested;
        _viewModel.ExitRequested += OnExitRequested;
    }

    #endregion

    #region Private Event Handlers

    private void OnNewProfileRequested()
    {
        CreateUserWindow createUserWindow = new(_userService)
        {
            Owner = this
        };

        createUserWindow.ShowDialog();
    }

    private void OnDeleteUserRequested()
    {
        if (_viewModel.SelectedUser == null)
        {
            return;
        }

        MessageBoxResult result = MessageBox.Show(
            $"Are you sure you want to delete user '{_viewModel.SelectedUser.Username}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        _viewModel.DeleteSelectedUser();
    }

    private void OnPlayRequested(User user)
    {
        GameWindow gameWindow = new(
            user,
            _gameService,
            _saveGameService,
            _statisticsService,
            _userService)
        {
            Owner = this
        };

        Hide();
        gameWindow.ShowDialog();
        Show();

        _viewModel.LoadUsers();
    }

    private void OnExitRequested()
    {
        Close();
    }

    #endregion
}