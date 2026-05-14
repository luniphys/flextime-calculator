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
            entry.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(10), selectFirst);
            
            void selectFirst()
            {
                entry.CursorPosition = 0;
                entry.SelectionLength = 1;
            };
        }
    }

    private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry) { return; }

        if (entry.Text.Length > 5)
        {
            return;
        }

        //bool isNumber = e.NewTextValue.All(character => char.IsDigit(character));

        bool isNumber = e.NewTextValue.Length == e.OldTextValue.Length &&
                        e.OldTextValue != e.NewTextValue &&
                        char.IsDigit(e.NewTextValue[entry.CursorPosition > 0 ? entry.CursorPosition - 1 : 0]);

        if (isNumber)
        {
            // Timing issue: Focused event fires sometimes before Entry is Focused -> No event handler execution -> Use Dispatch Timer
            entry.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), selectNext);

            void selectNext()
            {
                int cursor = entry.CursorPosition;
                if (cursor == 2)
                {
                    cursor = 3;
                }
                if (cursor < entry.Text.Length)
                {
                    entry.CursorPosition = cursor;
                    entry.SelectionLength = 1;
                }
            };
        }

        

        
    }
}
