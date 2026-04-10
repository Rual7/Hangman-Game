using System.Windows;

namespace Hangman_Game.Views;

public partial class InputDialog : Window
{
    #region Public Properties

    public string Message { get; set; }

    public string InputText { get; set; } = string.Empty;

    #endregion

    #region Constructors

    public InputDialog(string message)
    {
        InitializeComponent();

        Message = message;
        DataContext = this;

        Loaded += (_, _) => SaveNameTextBox.Focus();
    }

    #endregion

    #region Private Event Handlers

    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    #endregion
}