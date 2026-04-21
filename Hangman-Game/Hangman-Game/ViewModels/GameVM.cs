using Hangman_Game.Helpers;
using Hangman_Game.Mappers;
using Hangman_Game.Models;
using Hangman_Game.Services.Interfaces;
using Hangman_Game.ViewModels.Base;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Hangman_Game.ViewModels;

public class GameVM : BaseVM
{
    #region Fields

    private readonly IGameService _gameService;
    private readonly ISaveGameService _saveGameService;
    private readonly IStatisticsService _statisticsService;
    private readonly IUserService _userService;
    private readonly DispatcherTimer _timer;

    private GameSession? _currentSession;
    private string _selectedCategory = "All categories";
    private string _maskedWord = "_ _ _ _ _";
    private string _timeDisplay = "00:00";
    private string _statusMessage = "Choose a category and press START.";
    private bool _gameAlreadyCounted;

    private bool _isAwaitingRestart;
    private bool _suppressCloseConfirmationOnce;
    private int _displayCompletedLevels;

    private bool _isLevel1Failed;
    private bool _isLevel2Failed;
    private bool _isLevel3Failed;

    #endregion

    #region Collections

    public ObservableCollection<string> Categories { get; } = new();
    public ObservableCollection<SavedGame> UserSaves { get; } = new();
    public ObservableCollection<SaveMenuItemVM> SavedGamesMenuItems { get; } = new();
    public ObservableCollection<CategoryMenuItemVM> CategoryMenuItems { get; } = new();

    public ObservableCollection<char> KeyboardRow1 { get; } = new() { 'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P' };
    public ObservableCollection<char> KeyboardRow2 { get; } = new() { 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L' };
    public ObservableCollection<char> KeyboardRow3 { get; } = new() { 'Z', 'X', 'C', 'V', 'B', 'N', 'M' };

    #endregion

    #region Public Properties

    public User CurrentUser { get; }

    public MenuCommands Menu { get; }

    public GameSession? CurrentSession
    {
        get => _currentSession;
        set
        {
            if (SetProperty(ref _currentSession, value))
            {
                RefreshDerivedState();
                RaiseLetterCommandsCanExecute();
            }
        }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                SyncCategoryChecks(value);
                OnPropertyChanged(nameof(HintText));
            }
        }
    }

    public string MaskedWord
    {
        get => _maskedWord;
        set => SetProperty(ref _maskedWord, value);
    }

    public string TimeDisplay
    {
        get => _timeDisplay;
        set => SetProperty(ref _timeDisplay, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public int CurrentLevel => CurrentSession?.CurrentLevel ?? 0;
    public int WrongGuessesCount => CurrentSession?.WrongGuessesCount ?? 0;
    public int ConsecutiveWins => CurrentSession?.ConsecutiveWins ?? 0;

    public bool HasActiveSession => CurrentSession != null && !_isAwaitingRestart;
    public bool HasSavedGames => SavedGamesMenuItems.Count > 0;

    public string UsernameDisplay => CurrentUser.Username;
    public string UserAvatarPath => CurrentUser.AvatarFullPath;
    public string UserLevelDisplay => $"Level: {CurrentUser.Level}";

    public string HintText => HasActiveSession
        ? $"Hint: {CurrentSession!.Category}"
        : string.Equals(SelectedCategory, "All categories", StringComparison.OrdinalIgnoreCase)
            ? "Hint: All categories"
            : $"Hint: {SelectedCategory}";

    public bool IsLevel1Completed => _displayCompletedLevels >= 1;
    public bool IsLevel2Completed => _displayCompletedLevels >= 2;
    public bool IsLevel3Completed => _displayCompletedLevels >= 3;

    public bool IsLevel1Failed
    {
        get => _isLevel1Failed;
        set => SetProperty(ref _isLevel1Failed, value);
    }

    public bool IsLevel2Failed
    {
        get => _isLevel2Failed;
        set => SetProperty(ref _isLevel2Failed, value);
    }

    public bool IsLevel3Failed
    {
        get => _isLevel3Failed;
        set => SetProperty(ref _isLevel3Failed, value);
    }

    public string TimerFrameImagePath =>
        PathHelper.ToAbsolutePath(Path.Combine("Assets", "timerFrame.png"));

    public string HangmanImagePath
    {
        get
        {
            int stage = Math.Clamp(WrongGuessesCount, 0, 6);
            return PathHelper.ToAbsolutePath(Path.Combine("Assets", "Gallows", $"stage{stage}.png"));
        }
    }

    #endregion

    #region Commands

    public ICommand GuessLetterCommand { get; }

    #endregion

    #region Events

    public event Action? CancelRequested;
    public event Action? StatisticsRequested;
    public event Action? AboutRequested;
    public event Func<string?>? SaveNameRequested;
    public event Func<bool>? ConfirmCloseRequested;

    #endregion

    #region Constructors

    public GameVM(
        User currentUser,
        IGameService gameService,
        ISaveGameService saveGameService,
        IStatisticsService statisticsService,
        IUserService userService)
    {
        CurrentUser = currentUser;
        _gameService = gameService;
        _saveGameService = saveGameService;
        _statisticsService = statisticsService;
        _userService = userService;
        _gameAlreadyCounted = false;

        foreach (string category in _gameService.GetCategories())
        {
            Categories.Add(category);
        }

        BuildCategoryMenuItems();

        GuessLetterCommand = new RelayCommand(
            parameter => GuessLetter(parameter),
            parameter => CanGuessLetter(parameter));

        Menu = new MenuCommands(
            StartNewGame,
            SaveGame,
            ShowStatistics,
            CancelGame,
            ShowAbout);

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTimerTick;

        RefreshSaveList();
        RefreshDerivedState();
    }

    #endregion

    #region Public Window Methods

    public bool ConfirmWindowClose()
    {
        if (_suppressCloseConfirmationOnce)
        {
            _suppressCloseConfirmationOnce = false;
            return true;
        }

        if (!HasActiveSession)
        {
            return true;
        }

        return ConfirmCloseRequested?.Invoke() ?? true;
    }

    #endregion

    #region Private Game Flow Methods

    private void StartNewGame()
    {
        StopTimer();

        _isAwaitingRestart = false;
        _suppressCloseConfirmationOnce = false;
        _displayCompletedLevels = 0;
        ResetFailedIndicators();

        CurrentSession = _gameService.StartNewGame(CurrentUser.Username, SelectedCategory);
        _gameAlreadyCounted = false;

        RefreshDerivedState();
        RaiseLetterCommandsCanExecute();

        StatusMessage = "Game started. Choose letters.";
        StartTimer();
    }

    private void SaveGame()
    {
        if (CurrentSession == null)
        {
            StatusMessage = "There is no active game to save.";
            return;
        }
        _timer.Stop();
        string? saveName = SaveNameRequested?.Invoke();

        if (string.IsNullOrWhiteSpace(saveName))
        {
            StatusMessage = "Save cancelled.";
            _timer.Start();
            return;
        }

        SavedGame savedGame = GameMapper.ToSavedGame(CurrentSession, saveName.Trim());
        _saveGameService.SaveGame(savedGame);

        RefreshSaveList();
        StatusMessage = $"Game saved as '{savedGame.SaveName}'.";
        _timer.Start();
    }

    private void OpenSavedGame(string saveName)
    {
        SavedGame? savedGame = _saveGameService.LoadGame(CurrentUser.Username, saveName);

        if (savedGame == null)
        {
            StatusMessage = "Save not found.";
            return;
        }

        StopTimer();

        CurrentSession = GameMapper.ToGameSession(savedGame);
        SelectedCategory = CurrentSession.Category;
        _gameAlreadyCounted = false;
        _isAwaitingRestart = false;
        _suppressCloseConfirmationOnce = false;
        _displayCompletedLevels = CurrentSession.ConsecutiveWins;
        ResetFailedIndicators();

        RefreshDerivedState();
        RaiseLetterCommandsCanExecute();

        StatusMessage = $"Save '{savedGame.SaveName}' loaded.";
        StartTimer();
    }

    private void CancelGame()
    {
        if (HasActiveSession)
        {
            _timer.Stop();
            bool canClose = ConfirmCloseRequested?.Invoke() ?? true;

            if (!canClose)
            {
                _timer.Start();
                return;
            }

            _suppressCloseConfirmationOnce = true;
        }

        StopTimer();
        CancelRequested?.Invoke();
    }

    private void ShowStatistics()
    {
        _timer.Stop();
        StatisticsRequested?.Invoke();
        _timer.Start();
    }

    private void ShowAbout()
    {
        _timer.Stop();
        AboutRequested?.Invoke();
        _timer.Start();
    }

    private void GuessLetter(object? parameter)
    {
        if (CurrentSession == null || parameter == null || !HasActiveSession)
        {
            return;
        }

        char selectedLetter = ExtractLetter(parameter);
        bool wasCorrect = _gameService.GuessLetter(CurrentSession, selectedLetter);

        RefreshDerivedState();
        RaiseLetterCommandsCanExecute();

        if (_gameService.IsLevelWon(CurrentSession))
        {
            _gameService.StartNextLevel(CurrentSession);
            _displayCompletedLevels = Math.Max(_displayCompletedLevels, CurrentSession.ConsecutiveWins);

            if (_gameService.IsGameWon(CurrentSession))
            {
                HandleGameWon();
                return;
            }

            RefreshDerivedState();
            RaiseLetterCommandsCanExecute();
            StatusMessage = $"Level completed. Starting level {CurrentSession.CurrentLevel}.";
            StartTimer();
            return;
        }

        if (_gameService.IsLevelLost(CurrentSession))
        {
            HandleLevelFailed();
            return;
        }

        StatusMessage = wasCorrect
            ? $"Correct letter: {char.ToUpper(selectedLetter)}"
            : $"Wrong letter: {char.ToUpper(selectedLetter)}";
    }

    private bool CanGuessLetter(object? parameter)
    {
        if (CurrentSession == null || parameter == null || !HasActiveSession)
        {
            return false;
        }

        char selectedLetter = char.ToLower(ExtractLetter(parameter));

        return !CurrentSession.GuessedLetters.Contains(selectedLetter)
               && !CurrentSession.WrongLetters.Contains(selectedLetter)
               && !_gameService.IsLevelLost(CurrentSession)
               && !_gameService.IsLevelWon(CurrentSession);
    }

    private void HandleLevelFailed()
    {
        StopTimer();
        RegisterGameFinished(false);

        _isAwaitingRestart = true;

        switch (CurrentLevel)
        {
            case 1:
                IsLevel1Failed = true;
                break;
            case 2:
                IsLevel2Failed = true;
                break;
            case 3:
                IsLevel3Failed = true;
                break;
        }

        StatusMessage = "Level Failed. Select Category and press Start to play again.";

        RefreshDerivedState();
        RaiseLetterCommandsCanExecute();
    }

    private void HandleGameWon()
    {
        StopTimer();
        RegisterGameFinished(true);

        _isAwaitingRestart = true;
        _displayCompletedLevels = 3;
        ResetFailedIndicators();

        StatusMessage = "You Won! Select Category and press Start to play again.";

        RefreshDerivedState();
        RaiseLetterCommandsCanExecute();
    }

    #endregion

    #region Private Timer Methods

    private void StartTimer()
    {
        if (!HasActiveSession)
        {
            return;
        }

        UpdateTimeDisplay();
        _timer.Stop();
        _timer.Start();
    }

    private void StopTimer()
    {
        _timer.Stop();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (CurrentSession == null)
        {
            StopTimer();
            return;
        }

        if (CurrentSession.RemainingSeconds > 0)
        {
            CurrentSession.RemainingSeconds--;
        }

        UpdateTimeDisplay();

        if (CurrentSession.RemainingSeconds <= 0)
        {
            HandleLevelFailed();
        }
    }

    #endregion

    #region Private Save and Category Methods

    private void RefreshSaveList()
    {
        UserSaves.Clear();
        SavedGamesMenuItems.Clear();

        IEnumerable<SavedGame> saves = _saveGameService
            .GetAllSaves(CurrentUser.Username)
            .OrderByDescending(save => save.SavedAt);

        foreach (SavedGame save in saves)
        {
            UserSaves.Add(save);

            SavedGamesMenuItems.Add(new SaveMenuItemVM
            {
                Header = save.SaveName,
                Command = new RelayCommand(_ => OpenSavedGame(save.SaveName)),
                CommandParameter = save.SaveName
            });
        }

        OnPropertyChanged(nameof(HasSavedGames));
    }

    private void BuildCategoryMenuItems()
    {
        CategoryMenuItems.Clear();

        foreach (string category in Categories)
        {
            CategoryMenuItems.Add(new CategoryMenuItemVM
            {
                Header = category,
                IsChecked = string.Equals(category, SelectedCategory, StringComparison.OrdinalIgnoreCase),
                Command = new RelayCommand(_ => SelectCategory(category)),
                CommandParameter = category
            });
        }

        SyncCategoryChecks(SelectedCategory);
    }

    private void SyncCategoryChecks(string selectedCategory)
    {
        foreach (CategoryMenuItemVM categoryItem in CategoryMenuItems)
        {
            categoryItem.IsChecked = string.Equals(
                categoryItem.Header,
                selectedCategory,
                StringComparison.OrdinalIgnoreCase);
        }
    }

    private void SelectCategory(string category)
    {
        string previousCategory = SelectedCategory;

        if (HasActiveSession)
        {
            if (string.Equals(previousCategory, category, StringComparison.OrdinalIgnoreCase))
            {
                SyncCategoryChecks(previousCategory);
                return;
            }
            _timer.Stop();
            MessageBoxResult result = MessageBox.Show(
                "Changing category will reset your current progress. Continue?",
                "Confirm Category Change",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.OK)
            {
                SyncCategoryChecks(previousCategory);
                _timer.Start();
                return;
            }

            _gameService.ResetProgress(CurrentSession!, category);
            _gameAlreadyCounted = false;
            _displayCompletedLevels = 0;
            ResetFailedIndicators();
            _isAwaitingRestart = false;

            SelectedCategory = CurrentSession!.Category;
            RefreshDerivedState();
            RaiseLetterCommandsCanExecute();
            StartTimer();

            StatusMessage = $"Category changed to {CurrentSession.Category}. Progress reset.";
            return;
        }

        SelectedCategory = category;
        SyncCategoryChecks(category);

        if (!_isAwaitingRestart)
        {
            StatusMessage = $"Category selected: {category}";
        }
    }

    #endregion

    #region Private User Progress Methods

    private void RegisterGameFinished(bool won)
    {
        if (CurrentSession == null || _gameAlreadyCounted)
        {
            return;
        }

        _statisticsService.RegisterGamePlayed(
            CurrentSession.Username,
            CurrentSession.Category,
            won);

        _gameAlreadyCounted = true;
        UpdateUserProgress(won);
    }

    private void UpdateUserProgress(bool won)
    {
        CurrentUser.GamesPlayed++;

        if (won)
        {
            CurrentUser.GamesWon++;
            CurrentUser.Level++;
        }

        _userService.UpdateUserProgress(CurrentUser);
        OnPropertyChanged(nameof(UserLevelDisplay));
    }

    #endregion

    #region Private UI Refresh Methods

    private void RefreshDerivedState()
    {
        MaskedWord = CurrentSession == null
            ? "_ _ _ _ _"
            : _gameService.GetMaskedWord(CurrentSession);

        OnPropertyChanged(nameof(CurrentLevel));
        OnPropertyChanged(nameof(WrongGuessesCount));
        OnPropertyChanged(nameof(ConsecutiveWins));
        OnPropertyChanged(nameof(HasActiveSession));
        OnPropertyChanged(nameof(HangmanImagePath));
        OnPropertyChanged(nameof(HintText));
        OnPropertyChanged(nameof(IsLevel1Completed));
        OnPropertyChanged(nameof(IsLevel2Completed));
        OnPropertyChanged(nameof(IsLevel3Completed));
        OnPropertyChanged(nameof(IsLevel1Failed));
        OnPropertyChanged(nameof(IsLevel2Failed));
        OnPropertyChanged(nameof(IsLevel3Failed));
        OnPropertyChanged(nameof(UserLevelDisplay));

        UpdateTimeDisplay();
    }

    private void UpdateTimeDisplay()
    {
        if (CurrentSession == null)
        {
            TimeDisplay = "00:00";
            return;
        }

        TimeDisplay = CurrentSession.RemainingSeconds <= 0
            ? "Time's Up"
            : TimeSpan.FromSeconds(CurrentSession.RemainingSeconds).ToString(@"mm\:ss");
    }

    private void RaiseLetterCommandsCanExecute()
    {
        if (GuessLetterCommand is RelayCommand relayCommand)
        {
            relayCommand.RaiseCanExecuteChanged();
        }
    }

    private void ResetFailedIndicators()
    {
        IsLevel1Failed = false;
        IsLevel2Failed = false;
        IsLevel3Failed = false;
    }

    #endregion

    #region Private Helper Methods

    private char ExtractLetter(object parameter)
    {
        if (parameter is char character)
        {
            return character;
        }

        if (parameter is string text && text.Length == 1)
        {
            return text[0];
        }

        throw new InvalidOperationException("Invalid letter parameter.");
    }

    #endregion
}