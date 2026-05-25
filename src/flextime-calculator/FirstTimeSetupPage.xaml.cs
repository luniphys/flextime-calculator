namespace flextime_calculator;

public partial class FirstTimeSetupPage : ContentPage
{
    private uint _pageIndex = 0;

	public FirstTimeSetupPage()
	{
		InitializeComponent();
	}

    private void Entry_Focused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), () =>
            {
                entry.CursorPosition = 0;
                entry.SelectionLength = entry.Text?.Length ?? 0;
            });
        }
    }

    private void BackButton_Clicked(object sender, EventArgs e)
    {
        _pageIndex -= 1;
        updatePage();
    }

    private void NextButton_Clicked(object sender, EventArgs e)
    {
        _pageIndex += 1;
        updatePage();
    }

    private async void DoneButton_Clicked(object sender, EventArgs e)
    {
        Preferences.Set("usualComeTime", setupComeTime.ToString());
        Preferences.Set("usualGoTime", setupGoTime.ToString());
        Preferences.Set("weeklyHours", setupWeeklyHours.ToString());
        Preferences.Set("weeklyMinutes", setupWeeklyMinutes.ToString());
        Preferences.Set("smallBreak", setupSmallBreak.ToString());
        Preferences.Set("mainBreak", setupMainBreak.ToString());

        Preferences.Set("setupComplete", true);

        await Navigation.PopModalAsync();
    }

    void updatePage()
    {
        switch(_pageIndex)
        {
            case 0:
                comeGrid.IsEnabled = true;
                comeGrid.IsVisible = true;
                goGrid.IsEnabled = false;
                goGrid.IsVisible = false;
                weeklyGrid.IsEnabled = false;
                weeklyGrid.IsVisible = false;
                breakGrid.IsEnabled = false;
                breakGrid.IsVisible = false;
                break;

            case 1:
                comeGrid.IsEnabled = false;
                comeGrid.IsVisible = false;
                goGrid.IsEnabled = true;
                goGrid.IsVisible = true;
                weeklyGrid.IsEnabled = false;
                weeklyGrid.IsVisible = false;
                breakGrid.IsEnabled = false;
                breakGrid.IsVisible = false;
                break;

            case 2:
                comeGrid.IsEnabled = false;
                comeGrid.IsVisible = false;
                goGrid.IsEnabled = false;
                goGrid.IsVisible = false;
                weeklyGrid.IsEnabled = true;
                weeklyGrid.IsVisible = true;
                breakGrid.IsEnabled = false;
                breakGrid.IsVisible = false;
                break;

            case 3:
                comeGrid.IsEnabled = false;
                comeGrid.IsVisible = false;
                goGrid.IsEnabled = false;
                goGrid.IsVisible = false;
                weeklyGrid.IsEnabled = false;
                weeklyGrid.IsVisible = false;
                breakGrid.IsEnabled = true;
                breakGrid.IsVisible = true;
                break;
        }
    }
}