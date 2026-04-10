using Hangman_Game.Models;
using Hangman_Game.Services.Interfaces;
using Hangman_Game.ViewModels;
using System.Windows;

namespace Hangman_Game.Views;

public partial class CreateUserWindow : Window
{
    #region Fields

    private readonly CreateUserVM _viewModel;

    #endregion

    #region Constructors

    public CreateUserWindow(IUserService userService)
    {
        InitializeComponent();

        _viewModel = new CreateUserVM(userService);
        DataContext = _viewModel;

        _viewModel.ProfileCreated += OnProfileCreated;
        _viewModel.CancelRequested += OnCancelRequested;
    }

    #endregion

    #region Private Event Handlers

    private void OnProfileCreated(User user)
    {
        try
        {
            if (Owner is StartWindow startWindow && startWindow.DataContext is StartVM startViewModel)
            {
                startViewModel.AddUser(user);
            }

            DialogResult = true;
            Close();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                exception.Message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OnCancelRequested()
    {
        DialogResult = false;
        Close();
    }

    #endregion
}