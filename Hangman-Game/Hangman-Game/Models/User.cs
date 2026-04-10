using Hangman_Game.Helpers;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace Hangman_Game.Models;

public class User
{
    #region Profile Information

    public string Username { get; set; } = string.Empty;

    public string AvatarPath { get; set; } = string.Empty;

    [JsonIgnore]
    public string AvatarFullPath => PathHelper.ToAbsolutePath(AvatarPath);

    [JsonIgnore]
    public BitmapImage AvatarImage => ImageHelper.LoadBitmap(AvatarFullPath);

    #endregion

    #region Progress Information

    public int Level { get; set; } = 1;

    public int GamesPlayed { get; set; } = 0;

    public int GamesWon { get; set; } = 0;

    #endregion
}