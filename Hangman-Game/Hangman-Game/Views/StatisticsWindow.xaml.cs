using Hangman_Game.Services;
using Hangman_Game.ViewModels;
using System.Windows;

namespace Hangman_Game.Views;

public partial class StatisticsWindow : Window
{
    private readonly StatisticsVM _viewModel;

    public StatisticsWindow()
    {
        InitializeComponent();

        _viewModel = new StatisticsVM(new StatisticsService());
        DataContext = _viewModel;

        _viewModel.CloseRequested += () => Close();
    }
}