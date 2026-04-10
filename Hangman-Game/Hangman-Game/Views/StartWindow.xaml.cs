using Hangman_Game.Models;
using Hangman_Game.Services;
using Hangman_Game.ViewModels;
using System.Windows;

namespace Hangman_Game.Views;

public partial class StartWindow : Window
{
    #region Fields

    private readonly StartVM _viewModel;

    #endregion

    #region Constructors

    public StartWindow()
    {
        InitializeComponent();

        _viewModel = new StartVM(new UserService());
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
        CreateUserWindow createUserWindow = new()
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
        GameWindow gameWindow = new(user)
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