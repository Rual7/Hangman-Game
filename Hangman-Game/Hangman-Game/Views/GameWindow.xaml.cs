using Hangman_Game.Models;
using Hangman_Game.Services;
using Hangman_Game.Services.Interfaces;
using Hangman_Game.ViewModels;
using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Windows;

namespace Hangman_Game.Views;

public partial class GameWindow : Window
{
    public GameWindow(User user)
    {
        InitializeComponent();

        IGameService gameService = new GameService();
        ISaveGameService saveGameService = new SaveGameService();
        IStatisticsService statisticsService = new StatisticsService();

        var vm = new GameVM(user, gameService, saveGameService, statisticsService);

        vm.CancelRequested += () =>
        {
            Close();
        };

        vm.StatisticsRequested += () =>
        {
            var statisticsWindow = new StatisticsWindow
            {
                Owner = this
            };

            statisticsWindow.ShowDialog();
        };

        vm.AboutRequested += () =>
        {
            MessageBox.Show(
                "Hangman-Game\n\nStudent: Oncioiu Ionut-Raul\nGroup: 10LF243\nEmail: ionut.oncioiu@student.unitbv.ro",
                "About",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        };

        vm.SaveNameRequested += () =>
        {
            string result = Interaction.InputBox(
                "Enter a name for the save:",
                "Save Game",
                "MySave");

            return string.IsNullOrWhiteSpace(result) ? null : result.Trim();
        };

        vm.ConfirmCloseRequested += () =>
        {
            var result = MessageBox.Show(
                "You have a game in progress. Do you want to save it before exiting?",
                "Save Game",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel)
                return false;

            if (result == MessageBoxResult.Yes)
                vm.Menu.SaveGameCommand.Execute(null);

            return true;
        };

        DataContext = vm;

        Closing += GameWindow_Closing;
    }

    private void GameWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (DataContext is GameVM vm)
        {
            if (!vm.ConfirmWindowClose())
            {
                e.Cancel = true;
            }
        }
    }
}