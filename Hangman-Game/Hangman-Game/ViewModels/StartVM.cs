using Hangman_Game.Models;
using Hangman_Game.Services;
using Hangman_Game.Services.Interfaces;
using Hangman_Game.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Hangman_Game.ViewModels;

public class StartVM : BaseVM
{
    #region Fields

    private readonly IUserService _userService;
    private User? _selectedUser;

    #endregion

    #region Collections

    public ObservableCollection<User> Users { get; } = new();

    #endregion

    #region Bindable Properties

    public User? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (SetProperty(ref _selectedUser, value))
            {
                DeleteUserRelayCommand?.RaiseCanExecuteChanged();
                PlayRelayCommand?.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(HasSelectedUser));
            }
        }
    }

    public bool HasSelectedUser => SelectedUser != null;

    #endregion

    #region Commands

    public ICommand NewProfileCommand { get; }

    public ICommand DeleteUserCommand { get; }

    public ICommand PlayCommand { get; }

    public ICommand ExitCommand { get; }

    private RelayCommand? DeleteUserRelayCommand => DeleteUserCommand as RelayCommand;

    private RelayCommand? PlayRelayCommand => PlayCommand as RelayCommand;

    #endregion

    #region Events

    public event Action? NewProfileRequested;

    public event Action? DeleteUserRequested;

    public event Action<User>? PlayRequested;

    public event Action? ExitRequested;

    #endregion

    #region Constructors

    public StartVM(IUserService userService)
    {
        _userService = userService;

        NewProfileCommand = new RelayCommand(_ => NewProfileRequested?.Invoke());
        DeleteUserCommand = new RelayCommand(_ => DeleteUserRequested?.Invoke(), _ => SelectedUser != null);
        PlayCommand = new RelayCommand(_ => StartGame(), _ => SelectedUser != null);
        ExitCommand = new RelayCommand(_ => ExitRequested?.Invoke());

        LoadUsers();
    }

    #endregion

    #region Public Methods

    public void LoadUsers()
    {
        Users.Clear();

        foreach (User user in _userService.GetAllUsers())
        {
            Users.Add(user);
        }

        SelectedUser = null;
    }

    public void AddUser(User user)
    {
        _userService.AddUser(user);
        LoadUsers();

        User? addedUser = Users.FirstOrDefault(existingUser =>
            existingUser.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase));

        if (addedUser != null)
        {
            SelectedUser = addedUser;
        }
    }

    public void DeleteSelectedUser()
    {
        if (SelectedUser == null)
        {
            return;
        }

        string username = SelectedUser.Username;

        ISaveGameService saveGameService = new SaveGameService();
        IStatisticsService statisticsService = new StatisticsService();

        saveGameService.DeleteAllSaves(username);
        statisticsService.DeleteUserStatistics(username);
        _userService.DeleteUser(username);

        LoadUsers();
    }

    #endregion

    #region Private Methods

    private void StartGame()
    {
        if (SelectedUser != null)
        {
            PlayRequested?.Invoke(SelectedUser);
        }
    }

    #endregion
}