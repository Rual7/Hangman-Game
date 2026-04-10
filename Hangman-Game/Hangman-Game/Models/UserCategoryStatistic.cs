namespace Hangman_Game.Models;

public class UserCategoryStatistic
{
    #region Statistic Identity

    public string Username { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    #endregion

    #region Statistic Values

    public int GamesPlayed { get; set; }

    public int GamesWon { get; set; }

    #endregion
}