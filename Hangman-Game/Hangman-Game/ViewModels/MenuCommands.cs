using Hangman_Game.ViewModels.Base;
using System.Windows.Input;

namespace Hangman_Game.ViewModels;

public class MenuCommands
{
    public ICommand NewGameCommand { get; }
    public ICommand SaveGameCommand { get; }
    public ICommand StatisticsCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand AboutCommand { get; }

    public MenuCommands(
        Action newGameAction,
        Action saveGameAction,
        Action statisticsAction,
        Action cancelAction,
        Action aboutAction)
    {
        NewGameCommand = new RelayCommand(_ => newGameAction());
        SaveGameCommand = new RelayCommand(_ => saveGameAction());
        StatisticsCommand = new RelayCommand(_ => statisticsAction());
        CancelCommand = new RelayCommand(_ => cancelAction());
        AboutCommand = new RelayCommand(_ => aboutAction());
    }
}