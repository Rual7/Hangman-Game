using Hangman_Game.Models;
using Hangman_Game.Services;
using Hangman_Game.ViewModels;
using System.Windows;

namespace Hangman_Game.Views;

public partial class StartWindow : Window
{
    private readonly StartVM _viewModel;

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
    private void OnDeleteUserRequested()
    {
        if (_viewModel.SelectedUser == null)
            return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete user '{_viewModel.SelectedUser.Username}'?\n\n" +
            "This will delete:\n- all saved games\n- all statistics\n- avatar (if custom)",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        _viewModel.DeleteSelectedUser();
    }
    private void OnNewProfileRequested()
    {
        var createWindow = new CreateUserWindow();
        createWindow.Owner = this;
        createWindow.ShowDialog();
    }

    private void OnPlayRequested(User user)
    {
        string usernameToReselect = user.Username;

        var gameWindow = new GameWindow(user);

        gameWindow.Closed += (_, _) =>
        {
            _viewModel.LoadUsers();

            var refreshedUser = _viewModel.Users
                .FirstOrDefault(u => u.Username.Equals(usernameToReselect, StringComparison.OrdinalIgnoreCase));

            if (refreshedUser != null)
            {
                _viewModel.SelectedUser = refreshedUser;
            }

            Show();
            Activate();
        };

        Hide();
        gameWindow.Show();
    }

    private void OnExitRequested()
    {
        Close();
    }
}