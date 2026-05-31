using flextime_calculator.Constants;

namespace flextime_calculator;

public partial class FirstTimeSetupPage : ContentPage
{
    private int _pageIndex = 0;
    private readonly List<Grid> _gridList;


    public FirstTimeSetupPage()
	{
		InitializeComponent();
        _gridList = new List<Grid> { comeGrid, goGrid, weeklyGrid, breakGrid };

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
        if (_pageIndex > 0)
        {
            _pageIndex--;
            UpdatePage();
        }
    }


    /// <summary>
    /// Next button click navigates to next page.
    /// </summary>
    private void NextButton_Clicked(object sender, EventArgs e)
    {
        if (_pageIndex < _gridList.Count)
        {
            _pageIndex++;
            UpdatePage();
        }
    }

    #endregion



    #region Private (helper) methods

    /// <summary>
    /// Updates the enabled and visibility state of UI grids based on the current page index.
    /// </summary>
    private void UpdatePage()
    {
        for (int i = 0; i < _gridList.Count; i++)
        {
            _gridList[i].IsVisible = i == _pageIndex;
            _gridList[i].IsEnabled = i == _pageIndex;
        }

        BackButton.IsEnabled = _pageIndex > 0;
        BackButton.Opacity = _pageIndex == 0 ? 0.5 : 1;

        Color VSPurple = Color.FromRgb(80, 43, 212);
        NextButton.BackgroundColor = _pageIndex == _gridList.Count - 1 ? Colors.Green : VSPurple;
        NextButton.Text = _pageIndex == _gridList.Count - 1 ? "Fertig" : "Nächste";

        if (_pageIndex == _gridList.Count)
        {
            SaveSettings();
        }
    }


    /// <summary>
    /// Saves setup preferences and closes the setup modal.
    /// </summary>
    private async void SaveSettings()
    {
        Preferences.Set(PreferenceKeys.UsualComeTime, setupComeTime.Time.ToString());
        Preferences.Set(PreferenceKeys.UsualGoTime, setupGoTime.Time.ToString());
        Preferences.Set(PreferenceKeys.WeeklyHours, setupWeeklyHours.Text);
        Preferences.Set(PreferenceKeys.WeeklyMinutes, setupWeeklyMinutes.Text);
        Preferences.Set(PreferenceKeys.SmallBreak, setupSmallBreak.Text);
        Preferences.Set(PreferenceKeys.MainBreak, setupMainBreak.Text);

        Preferences.Set(PreferenceKeys.SetupComplete, true);

        await Navigation.PopModalAsync();
    }

    #endregion
}