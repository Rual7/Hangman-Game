using Hangman_Game.Models;
using Hangman_Game.Services.Interfaces;
using Hangman_Game.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Hangman_Game.ViewModels;

public class StartVM : BaseVM
{
    private readonly IUserService _userService;
    private User? _selectedUser;

    public ObservableCollection<User> Users { get; } = new();

    public User? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (SetProperty(ref _selectedUser, value))
            {
                DeleteUserCommandAsRelay?.RaiseCanExecuteChanged();
                PlayCommandAsRelay?.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(HasSelectedUser));
            }
        }
    }

    public bool HasSelectedUser => SelectedUser != null;

    public ICommand NewProfileCommand { get; }
    public ICommand DeleteUserCommand { get; }
    public ICommand PlayCommand { get; }
    public ICommand ExitCommand { get; }

    private RelayCommand? DeleteUserCommandAsRelay => DeleteUserCommand as RelayCommand;
    private RelayCommand? PlayCommandAsRelay => PlayCommand as RelayCommand;

    public event Action? NewProfileRequested;
    public event Action<User>? PlayRequested;
    public event Action? ExitRequested;

    public StartVM(IUserService userService)
    {
        _userService = userService;

        NewProfileCommand = new RelayCommand(_ => NewProfileRequested?.Invoke());
        DeleteUserCommand = new RelayCommand(_ => DeleteSelectedUser(), _ => SelectedUser != null);
        PlayCommand = new RelayCommand(_ => StartGame(), _ => SelectedUser != null);
        ExitCommand = new RelayCommand(_ => ExitRequested?.Invoke());

        LoadUsers();
    }

    public void LoadUsers()
    {
        Users.Clear();

        foreach (var user in _userService.GetAllUsers())
            Users.Add(user);

        SelectedUser = null;
    }

    public void AddUser(User user)
    {
        _userService.AddUser(user);
        LoadUsers();

        foreach (var existingUser in Users)
        {
            if (existingUser.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase))
            {
                SelectedUser = existingUser;
                break;
            }
        }
    }

    private void DeleteSelectedUser()
    {
        if (SelectedUser == null)
            return;

        _userService.DeleteUser(SelectedUser.Username);
        LoadUsers();
    }

    private void StartGame()
    {
        if (SelectedUser != null)
            PlayRequested?.Invoke(SelectedUser);
    }
}