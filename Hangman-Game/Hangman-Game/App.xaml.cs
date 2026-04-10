using Hangman_Game.Services;
using Hangman_Game.Services.Interfaces;
using Hangman_Game.Views;
using System.Windows;

namespace Hangman_Game;

public partial class App : Application
{
    #region Fields

    private IUserService _userService = null!;
    private IGameService _gameService = null!;
    private ISaveGameService _saveGameService = null!;
    private IStatisticsService _statisticsService = null!;

    #endregion

    #region Protected Application Methods

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ConfigureServices();

        StartWindow startWindow = new(
            _userService,
            _saveGameService,
            _statisticsService,
            _gameService);

        startWindow.Show();
    }

    #endregion

    #region Private Initialization Methods

    private void ConfigureServices()
    {
        _userService = new UserService();
        _gameService = new GameService();
        _saveGameService = new SaveGameService();
        _statisticsService = new StatisticsService();
    }

    #endregion
}