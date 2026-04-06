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
        _viewModel.PlayRequested += OnPlayRequested;
        _viewModel.ExitRequested += OnExitRequested;
    }

    private void OnNewProfileRequested()
    {
        var createWindow = new CreateUserWindow();
        createWindow.Owner = this;
        createWindow.ShowDialog();
    }

    private void OnPlayRequested(User user)
    {
        MessageBox.Show(
            $"TO BE IMPLEMENTED, Start game for : {user.Username}.",
            "Play",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void OnExitRequested()
    {
        Close();
    }
}
