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
    #region Fields

    private readonly IUserService _userService;

    private string _username = string.Empty;
    private AvatarItem? _selectedAvatar;
    private string _errorMessage = string.Empty;

    #endregion

    #region Collections

    public ObservableCollection<AvatarItem> AvailableAvatars { get; } = new();

    #endregion

    #region Bindable Properties

    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                CreateProfileRelayCommand?.RaiseCanExecuteChanged();
            }
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
                CreateProfileRelayCommand?.RaiseCanExecuteChanged();
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

    #endregion

    #region Commands

    public ICommand BrowseAvatarCommand { get; }

    public ICommand CreateProfileCommand { get; }

    public ICommand CancelCommand { get; }

    private RelayCommand? CreateProfileRelayCommand => CreateProfileCommand as RelayCommand;

    #endregion

    #region Events

    public event Action<User>? ProfileCreated;

    public event Action? CancelRequested;

    #endregion

    #region Constructors

    public CreateUserVM(IUserService userService)
    {
        _userService = userService;

        LoadAvailableAvatars();

        BrowseAvatarCommand = new RelayCommand(_ => BrowseAvatar());
        CreateProfileCommand = new RelayCommand(_ => CreateProfile(), _ => CanCreateProfile());
        CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke());
    }

    #endregion

    #region Private Avatar Methods

    private void LoadAvailableAvatars()
    {
        AvailableAvatars.Clear();

        foreach (string avatarPath in _userService.GetAllAvailableAvatars())
        {
            AvailableAvatars.Add(new AvatarItem(avatarPath));
        }
    }

    private void BrowseAvatar()
    {
        OpenFileDialog dialog = new()
        {
            Title = "Select avatar",
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.gif",
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            string customAvatarsFolderPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "Avatars",
                "Custom");

            Directory.CreateDirectory(customAvatarsFolderPath);

            string sourceFilePath = Path.GetFullPath(dialog.FileName);
            string fileName = Path.GetFileName(sourceFilePath);
            string destinationFilePath = Path.GetFullPath(Path.Combine(customAvatarsFolderPath, fileName));

            string relativePath = PathHelper.ToRelativePath(destinationFilePath);

            bool isSameFile = string.Equals(
                sourceFilePath,
                destinationFilePath,
                StringComparison.OrdinalIgnoreCase);

            if (!isSameFile && !File.Exists(destinationFilePath))
            {
                File.Copy(sourceFilePath, destinationFilePath);
            }

            AvatarItem? existingAvatar = AvailableAvatars.FirstOrDefault(avatar =>
                avatar.RelativePath.Equals(relativePath, StringComparison.OrdinalIgnoreCase));

            if (existingAvatar == null)
            {
                existingAvatar = new AvatarItem(relativePath);
                AvailableAvatars.Add(existingAvatar);
            }

            SelectedAvatar = existingAvatar;
            ErrorMessage = string.Empty;
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Could not load avatar: {exception.Message}";
        }
    }

    #endregion

    #region Private Profile Methods

    private bool CanCreateProfile()
    {
        return !string.IsNullOrWhiteSpace(Username)
               && SelectedAvatar != null;
    }

    private void CreateProfile()
    {
        ErrorMessage = string.Empty;
        string trimmedUsername = Username.Trim();

        if (!IsValidUsername(trimmedUsername))
        {
            ErrorMessage = "The username can only contain letters, numbers, and underscores, with no spaces.";
            return;
        }

        if (_userService.UserExists(trimmedUsername))
        {
            ErrorMessage = "A user with this name already exists.";
            return;
        }

        if (SelectedAvatar == null)
        {
            ErrorMessage = "You must select an avatar.";
            return;
        }

        User newUser = new()
        {
            Username = trimmedUsername,
            AvatarPath = SelectedAvatar.RelativePath,
            Level = 1,
            GamesPlayed = 0,
            GamesWon = 0
        };

        ProfileCreated?.Invoke(newUser);
    }

    #endregion

    #region Private Validation Methods

    private bool IsValidUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        if (username.Contains(' '))
        {
            return false;
        }

        return username.All(character => char.IsLetterOrDigit(character) || character == '_');
    }

    #endregion
}