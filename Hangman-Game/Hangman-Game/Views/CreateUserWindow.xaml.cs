using Hangman_Game.Models;
using Hangman_Game.Services;
using Hangman_Game.ViewModels;
using System.Windows;

namespace Hangman_Game.Views;

public partial class CreateUserWindow : Window
{
    private readonly CreateUserVM _viewModel;

    public CreateUserWindow()
    {
        InitializeComponent();

        _viewModel = new CreateUserVM(new UserService());
        DataContext = _viewModel;

        _viewModel.ProfileCreated += OnProfileCreated;
        _viewModel.CancelRequested += OnCancelRequested;
    }

    private void OnProfileCreated(User user)
    {
        try
        {
            var startWindow = Owner as StartWindow;
            if (startWindow?.DataContext is StartVM startVm)
            {
                startVm.AddUser(user);
            }

            DialogResult = true;
            Close();
        }
        catch (System.Exception ex)
        {
            MessageBox.Show(
                ex.Message,
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
}
