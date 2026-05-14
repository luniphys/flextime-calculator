namespace end_of_work_time;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

    private void OnEntryFocused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry && entry.Text is not null)
        {
            // Timing issue: Focused event fires sometimes before Entry is Focused -> No event handler execution -> Use Dispatch Timer
            entry.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(10), selectAllText);
            
            void selectAllText()
            {
                entry.CursorPosition = 0;
                entry.SelectionLength = entry.Text.Length;
            };
        }
    }

    private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry) { return; }

        if (entry.Text.Length > 5) { return; }

        bool isValid = e.NewTextValue.All(character => char.IsDigit(character) || character == ':');

        if (entry.Text.Length == 2)
        {
            entry.Text = e.NewTextValue + ":";
            entry.CursorPosition = entry.Text.Length;
        }

        if (!isValid)
        {
            entry.Text = "";
            return;
        }
    }
}
