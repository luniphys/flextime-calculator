namespace flextime_calculator;

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
            UpdateDelta();
            CalculateFeierabend();
        }
    }

    private void UsualTimePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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

            UpdateDelta();
            CalculateFeierabend();
        }
    }

    private void UpdateDelta()
    {
        TimeSpan smallBreak = TimeSpan.FromMinutes(15);
        TimeSpan mainBreak = TimeSpan.FromMinutes(30);
        TimeSpan totalBreak = smallBreak + mainBreak;

        TimeSpan mondayDuration = (TimeSpan)goMon.Time! - (TimeSpan)comeMon.Time! - totalBreak;
        TimeSpan tuesdayDuration = (TimeSpan)goTue.Time! - (TimeSpan)comeTue.Time! - totalBreak;
        TimeSpan wednesdayDuration = (TimeSpan)goWed.Time! - (TimeSpan)comeWed.Time! - totalBreak;
        TimeSpan thursdayDuration = (TimeSpan)goThu.Time! - (TimeSpan)comeThu.Time! - totalBreak;

        double dailyTotal = VerifyTime(dailyHours.Text, dailyMinutes.Text);
        TimeSpan totalDailyHours = TimeSpan.FromHours(dailyTotal);

        double mondayDeltaHours = (mondayDuration - totalDailyHours).TotalHours;
        double mondayDeltaMinutes = (mondayDuration - totalDailyHours).Minutes;
        // deltaMon.Text = ;
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

        double weeklyTotal = VerifyTime(weeklyHours.Text, weeklyMinutes.Text);
        TimeSpan totalWeeklyHours = TimeSpan.FromHours(weeklyTotal);
		TimeSpan fridayHours = totalWeeklyHours - fourDayDuration;

		TimeSpan feierAbendWeek = (TimeSpan)comeFri.Time! + fridayHours - smallBreak;

		TimeSpan miniumumFriday = new TimeSpan(12, 0, 0);
		if (feierAbendWeek < miniumumFriday)
		{
            feierAbendWeek = miniumumFriday;
		}

        feierabendTimeWeek.Text = $"{(int)feierAbendWeek.TotalHours}:{feierAbendWeek.Minutes}";

        double dailyTotal = VerifyTime(dailyHours.Text, dailyMinutes.Text);
        TimeSpan totalDailyHours = TimeSpan.FromHours(dailyTotal);
        TimeSpan feierAbendDay = (TimeSpan)comeDay.Time! + totalDailyHours + totalBreak;

        feierabendTimeDay.Text = $"{(int)feierAbendDay.TotalHours}:{feierAbendDay.Minutes}";
    }

    double VerifyTime(string hours, string minutes)
    {
        double hoursDouble = 0.0;
        double minutesDouble = 0.0;

        if (double.TryParse(hours, out double parsedHours))
        {

            hoursDouble = parsedHours;
        }

        if (double.TryParse(minutes, out double parsedMinutes))
        {
            if (0 <= parsedHours && parsedMinutes <= 60.0)
            {
                minutesDouble = parsedMinutes;
            }
        }

        return hoursDouble + minutesDouble / 60;
    }
}

// TODO: Remove .NET Intro animation
// TODO: Moving settings panel by swiping left/right

