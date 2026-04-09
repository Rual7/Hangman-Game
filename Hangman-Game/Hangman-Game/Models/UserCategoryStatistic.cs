namespace Hangman_Game.Models;

public class UserCategoryStatistic
{
    public string Username { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
}