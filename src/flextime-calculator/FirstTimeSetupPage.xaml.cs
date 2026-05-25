namespace flextime_calculator;

public partial class FirstTimeSetupPage : ContentPage
{
    private uint _pageIndex = 0;


	public FirstTimeSetupPage()
	{
		InitializeComponent();
	}


    #region Event handlers

    /// <summary>
    /// Clicking Entry selects all text.
    /// </summary>
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


    /// <summary>
    /// Back button click navigates to previous page.
    /// </summary>
    private void BackButton_Clicked(object sender, EventArgs e)
    {
        _pageIndex -= 1;
        updatePage();
    }


    /// <summary>
    /// Next button click navigates to next page.
    /// </summary>
    private void NextButton_Clicked(object sender, EventArgs e)
    {
        _pageIndex += 1;
        updatePage();
    }

    #endregion



    #region Private (helper) methods

    /// <summary>
    /// Updates the enabled and visibility state of UI grids based on the current page index.
    /// </summary>
    private void updatePage()
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

                BackButton.IsEnabled = false;
                BackButton.Opacity = 0.5;
                BackButton.TextColor = Colors.White;
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

                BackButton.IsEnabled = true;
                BackButton.Opacity = 1;
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

                NextButton.BackgroundColor = Color.FromRgb(80, 43, 212);
                NextButton.Text = "Next";
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

                NextButton.BackgroundColor = Colors.Green;
                NextButton.Text = "Done";

                break;

            case 4:
                UploadSettings();
                break;
        }
    }


    /// <summary>
    /// Saves setup preferences and closes the setup modal.
    /// </summary>
    private async void UploadSettings()
    {
        Preferences.Set("usualComeTime", setupComeTime.Time.ToString());
        Preferences.Set("usualGoTime", setupGoTime.Time.ToString());
        Preferences.Set("weeklyHours", setupWeeklyHours.Text);
        Preferences.Set("weeklyMinutes", setupWeeklyMinutes.Text);
        Preferences.Set("smallBreak", setupSmallBreak.Text);
        Preferences.Set("mainBreak", setupMainBreak.Text);

        Preferences.Set("setupComplete", true);

        await Navigation.PopModalAsync();
    }

    #endregion
}