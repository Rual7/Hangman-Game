using Hangman_Game.ViewModels.Base;
using System.Windows.Input;

namespace Hangman_Game.ViewModels;

public class MenuCommands
{
    #region Public Properties

    public ICommand NewGameCommand { get; }

    public ICommand SaveGameCommand { get; }

    public ICommand StatisticsCommand { get; }

    public ICommand CancelCommand { get; }

    public ICommand AboutCommand { get; }

    #endregion

    #region Constructors

    public MenuCommands(
        Action onNewGame,
        Action onSaveGame,
        Action onShowStatistics,
        Action onCancel,
        Action onShowAbout)
    {
        NewGameCommand = new RelayCommand(_ => onNewGame());
        SaveGameCommand = new RelayCommand(_ => onSaveGame());
        StatisticsCommand = new RelayCommand(_ => onShowStatistics());
        CancelCommand = new RelayCommand(_ => onCancel());
        AboutCommand = new RelayCommand(_ => onShowAbout());
    }

    #endregion
}