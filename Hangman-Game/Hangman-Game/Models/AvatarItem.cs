using Hangman_Game.Helpers;

namespace Hangman_Game.Models;

public class AvatarItem
{
    public AvatarItem(string relativePath)
    {
        RelativePath = relativePath;
    }

    public string RelativePath { get; set; } = string.Empty;

    public string FullPath => PathHelper.ToAbsolutePath(RelativePath);
}