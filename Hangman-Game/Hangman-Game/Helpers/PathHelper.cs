using System.IO;

namespace Hangman_Game.Helpers;

public static class PathHelper
{
    #region Public Path Utilities

    public static string GetProjectRoot()
    {
        return AppDomain.CurrentDomain.BaseDirectory;
    }

    public static string EnsureDirectory(string relativeFolder)
    {
        string fullPath = Path.Combine(GetProjectRoot(), relativeFolder);

        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        return fullPath;
    }

    public static string EnsureFileExists(string relativePath, string defaultContent = "")
    {
        string fullPath = Path.Combine(GetProjectRoot(), relativePath);
        string? directoryPath = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        if (!File.Exists(fullPath))
        {
            File.WriteAllText(fullPath, defaultContent);
        }

        return fullPath;
    }

    public static string ToRelativePath(string fullPath)
    {
        string basePath = GetProjectRoot();

        Uri baseUri = new(AppendDirectorySeparatorChar(basePath));
        Uri fullUri = new(fullPath);

        string relativePath = Uri.UnescapeDataString(baseUri.MakeRelativeUri(fullUri).ToString());
        return relativePath.Replace('/', Path.DirectorySeparatorChar);
    }

    public static string ToAbsolutePath(string relativePath)
    {
        return Path.Combine(GetProjectRoot(), relativePath);
    }

    #endregion

    #region Private Helpers

    private static string AppendDirectorySeparatorChar(string path)
    {
        if (!path.EndsWith(Path.DirectorySeparatorChar))
        {
            return path + Path.DirectorySeparatorChar;
        }

        return path;
    }

    #endregion
}