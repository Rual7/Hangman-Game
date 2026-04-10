using System.IO;
using System.Windows.Media.Imaging;

namespace Hangman_Game.Helpers;

public static class ImageHelper
{
    #region Public Methods

    public static BitmapImage LoadBitmap(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            return new BitmapImage();
        }

        byte[] imageBytes = File.ReadAllBytes(imagePath);

        using MemoryStream memoryStream = new(imageBytes);

        BitmapImage bitmap = new();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = memoryStream;
        bitmap.EndInit();
        bitmap.Freeze();

        return bitmap;
    }

    #endregion
}