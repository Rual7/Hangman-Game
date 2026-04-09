using Hangman_Game.Helpers;
using Hangman_Game.Models;
using Hangman_Game.Services.Interfaces;
using Hangman_Game.ViewModels.Base;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace Hangman_Game.ViewModels;

public class CreateUserVM : BaseVM
{
    private readonly IUserService _userService;

    private string _username = string.Empty;
    private AvatarItem? _selectedAvatar;
    private string _errorMessage = string.Empty;

    public ObservableCollection<AvatarItem> AvailableAvatars { get; } = new();

    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
                CreateProfileCommandAsRelay?.RaiseCanExecuteChanged();
        }
    }

    public AvatarItem? SelectedAvatar
    {
        get => _selectedAvatar;
        set
        {
            if (SetProperty(ref _selectedAvatar, value))
            {
                OnPropertyChanged(nameof(SelectedAvatarPath));
                OnPropertyChanged(nameof(SelectedAvatarFullPath));
                CreateProfileCommandAsRelay?.RaiseCanExecuteChanged();
            }
        }
    }

    public string SelectedAvatarPath => SelectedAvatar?.RelativePath ?? string.Empty;
    public string SelectedAvatarFullPath => SelectedAvatar?.FullPath ?? string.Empty;

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand BrowseAvatarCommand { get; }
    public ICommand CreateProfileCommand { get; }
    public ICommand CancelCommand { get; }

    private RelayCommand? CreateProfileCommandAsRelay => CreateProfileCommand as RelayCommand;

    public event Action<User>? ProfileCreated;
    public event Action? CancelRequested;

    public CreateUserVM(IUserService userService)
    {
        _userService = userService;

        LoadAvailableAvatars();

        BrowseAvatarCommand = new RelayCommand(_ => BrowseAvatar());
        CreateProfileCommand = new RelayCommand(_ => CreateProfile(), _ => CanCreateProfile());
        CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke());
    }

    private void LoadAvailableAvatars()
    {
        AvailableAvatars.Clear();

        foreach (var avatar in _userService.GetAllAvailableAvatars())
        {
            AvailableAvatars.Add(new AvatarItem(avatar));
        }
    }

    private bool CanCreateProfile()
    {
        return !string.IsNullOrWhiteSpace(Username)
               && SelectedAvatar != null;
    }

    private void BrowseAvatar()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select avatar",
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.gif",
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            string avatarsFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "Avatars",
                "Custom");

            Directory.CreateDirectory(avatarsFolder);

            string sourcePath = Path.GetFullPath(dialog.FileName);
            string fileName = Path.GetFileName(sourcePath);
            string destinationPath = Path.GetFullPath(Path.Combine(avatarsFolder, fileName));

            string relativePath = PathHelper.ToRelativePath(destinationPath);

            bool sameFile = string.Equals(sourcePath, destinationPath, StringComparison.OrdinalIgnoreCase);

            if (!sameFile && !File.Exists(destinationPath))
            {
                File.Copy(sourcePath, destinationPath);
            }

            var existingAvatar = AvailableAvatars
                .FirstOrDefault(a => a.RelativePath.Equals(relativePath, StringComparison.OrdinalIgnoreCase));

            if (existingAvatar == null)
            {
                existingAvatar = new AvatarItem(relativePath);
                AvailableAvatars.Add(existingAvatar);
            }

            SelectedAvatar = existingAvatar;
            ErrorMessage = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load avatar: {ex.Message}";
        }
    }

    private void CreateProfile()
    {
        ErrorMessage = string.Empty;
        string trimmedName = Username.Trim();

        if (!IsValidUsername(trimmedName))
        {
            ErrorMessage = "The username can only contain letters, numbers, and underscores, with no spaces.";
            return;
        }

        if (_userService.UserExists(trimmedName))
        {
            ErrorMessage = "A user with this name already exists.";
            return;
        }

        if (SelectedAvatar == null)
        {
            ErrorMessage = "You must select an avatar.";
            return;
        }

        var user = new User
        {
            Username = trimmedName,
            AvatarPath = SelectedAvatar.RelativePath,
            Level = 1,
            GamesPlayed = 0,
            GamesWon = 0
        };

        ProfileCreated?.Invoke(user);
    }

    private bool IsValidUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        if (username.Contains(' '))
            return false;

        return username.All(ch => char.IsLetterOrDigit(ch) || ch == '_');
    }
}