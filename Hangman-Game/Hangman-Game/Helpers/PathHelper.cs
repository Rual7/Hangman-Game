using System.IO;

namespace Hangman_Game.Helpers;

public class PathHelper
{
    public static string GetProjectRoot()
    {
        return AppDomain.CurrentDomain.BaseDirectory;
    }

    public static string EnsureDirectory(string relativeFolder)
    {
        string fullPath = Path.Combine(GetProjectRoot(), relativeFolder);
        if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);

        return fullPath;
    }

    public static string EnsureFileExists(string relativePath, string defaultContent = "")
    {
        string fullPath = Path.Combine(GetProjectRoot(), relativePath);

        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
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

        Uri baseUri = new Uri(AppendDirectorySeparatorChar(basePath));
        Uri fullUri = new Uri(fullPath);

        string relative = Uri.UnescapeDataString(baseUri.MakeRelativeUri(fullUri).ToString());
        return relative.Replace('/', Path.DirectorySeparatorChar);
    }

    public static string ToAbsolutePath(string relativePath)
    {
        return Path.Combine(GetProjectRoot(), relativePath);
    }

    private static string AppendDirectorySeparatorChar(string path)
    {
        if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            return path + Path.DirectorySeparatorChar;
        return path;
    }
}
