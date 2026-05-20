namespace end_of_work_time;

public partial class MainPage : ContentPage
{
	private bool _settingsOpen = false;
    private bool _weekMode = true;

	public MainPage()
	{
		InitializeComponent();
	}

    private void SettingsButton_Clicked(object sender, EventArgs e)
    {
        double panelWidth = 0.85;
        uint animationDuration = 100;

        if (_settingsOpen)
		{
			_settingsOpen = false;
            dimOverlay.IsVisible = false;
            mainPageGrid.IsEnabled = true;
            dayPageGrid.IsEnabled = true;
            settingsPanel.Animate("close", v => settingsPanel.WidthRequest = v, start: this.Width * panelWidth, end: 0, length: animationDuration, finished: (v, c) => settingsPanel.IsVisible = false);
        }
		else
		{
            _settingsOpen = true;
            dimOverlay.IsVisible = true;
            settingsPanel.IsVisible = true;
            mainPageGrid.IsEnabled = false;
            dayPageGrid.IsEnabled = false;
            settingsPanel.Animate("open", v => settingsPanel.WidthRequest = v, start: 0, end: this.Width * panelWidth, length: animationDuration);
        }
    }

    private void SwitchButton_Clicked(object sender, EventArgs e)
    {
        if (_weekMode)
        {
            switchButton.Text = "Week";
            mainPageGrid.IsEnabled = false;
            mainPageGrid.IsVisible = false;
            _weekMode = false;

            dayPageGrid.IsEnabled = true;
            dayPageGrid.IsVisible = true;
        }
        else
        {
            switchButton.Text = "Day";
            mainPageGrid.IsEnabled = true;
            mainPageGrid.IsVisible = true;
            _weekMode = true;

            dayPageGrid.IsEnabled = false;
            dayPageGrid.IsVisible = false;
        }
    }


    private void TimePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
		if (sender is TimePicker picker)
		{
			CalculateFeierabend();
        }
    }

    private void UsuaTimePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is TimePicker picker)
        {
			comeMon.Time = (TimeSpan)usualComeTime.Time!;
            comeTue.Time = (TimeSpan)usualComeTime.Time!;
            comeWed.Time = (TimeSpan)usualComeTime.Time!;
            comeThu.Time = (TimeSpan)usualComeTime.Time!;
            comeFri.Time = (TimeSpan)usualComeTime.Time!;

            goMon.Time = (TimeSpan)usualGoTime.Time!;
            goTue.Time = (TimeSpan)usualGoTime.Time!;
            goWed.Time = (TimeSpan)usualGoTime.Time!;
            goThu.Time = (TimeSpan)usualGoTime.Time!;

            CalculateFeierabend();
        }
    }

    private void CalculateFeierabend()
	{
		TimeSpan smallBreak = TimeSpan.FromMinutes(15);
        TimeSpan mainBreak = TimeSpan.FromMinutes(30);
		TimeSpan totalBreak = smallBreak + mainBreak;

        TimeSpan mondayDuration = (TimeSpan)goMon.Time! - (TimeSpan)comeMon.Time! - totalBreak;
        TimeSpan tuesdayDuration = (TimeSpan)goTue.Time! - (TimeSpan)comeTue.Time! - totalBreak;
        TimeSpan wednesdayDuration = (TimeSpan)goWed.Time! - (TimeSpan)comeWed.Time! - totalBreak;
        TimeSpan thursdayDuration = (TimeSpan)goThu.Time! - (TimeSpan)comeThu.Time! - totalBreak;

		TimeSpan fourDayDuration = mondayDuration + tuesdayDuration + wednesdayDuration + thursdayDuration;

		TimeSpan totalWeeklyHours = TimeSpan.FromHours(weeklyHours.Value);
		TimeSpan fridayHours = totalWeeklyHours - fourDayDuration;

		TimeSpan feierAbendWeek = (TimeSpan)comeFri.Time! + fridayHours - smallBreak;

		TimeSpan miniumumFriday = new TimeSpan(12, 0, 0);
		if (feierAbendWeek < miniumumFriday)
		{
            feierAbendWeek = miniumumFriday;
		}

        feierabendTimeWeek.Text = $"{(int)feierAbendWeek.TotalHours}:{feierAbendWeek.Minutes}";


        TimeSpan totalDailyHours = TimeSpan.FromHours(dailyHours.Value);
        TimeSpan feierAbendDay = (TimeSpan)comeDay.Time! + totalDailyHours + totalBreak;

        feierabendTimeDay.Text = $"{(int)feierAbendDay.TotalHours}:{feierAbendDay.Minutes}";
    }
}

// TODO: Remove .NET Intro animation
// TODO: Moving settings panel by swiping left/right

