using Hangman_Game.Helpers;
using System.Windows.Media.Imaging;

namespace Hangman_Game.Models;

public class AvatarItem
{
    #region Constructors

    public AvatarItem(string relativePath)
    {
        RelativePath = relativePath;
    }

    #endregion

    #region Public Properties

    public string RelativePath { get; set; } = string.Empty;

    public string FullPath => PathHelper.ToAbsolutePath(RelativePath);

    public BitmapImage AvatarImage => ImageHelper.LoadBitmap(FullPath);

    #endregion
}