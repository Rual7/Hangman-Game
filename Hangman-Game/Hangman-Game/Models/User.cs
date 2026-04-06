using Hangman_Game.Helpers;
using System.Text.Json.Serialization;

namespace Hangman_Game.Models;

public class User
{
    public string Username { get; set; } = string.Empty;
    public string AvatarPath { get; set; } = string.Empty;

    public int Level { get; set; } = 1;
    public int GamesPlayed { get; set; } = 0;
    public int GamesWon { get; set; } = 0;

    [JsonIgnore]
    public string AvatarFullPath => PathHelper.ToAbsolutePath(AvatarPath);
}