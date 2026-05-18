namespace end_of_work_time;

public partial class MainPage : ContentPage
{
	private bool _settingsOpen = false;

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
            daysGrid.IsEnabled = true;
            settingsPanel.Animate("close", v => settingsPanel.WidthRequest = v, start: this.Width * panelWidth, end: 0, length: animationDuration, finished: (v, c) => settingsPanel.IsVisible = false);
        }
		else
		{
            _settingsOpen = true;
            dimOverlay.IsVisible = true;
            settingsPanel.IsVisible = true;
            daysGrid.IsEnabled = false;
            settingsPanel.Animate("open", v => settingsPanel.WidthRequest = v, start: 0, end: this.Width * panelWidth, length: animationDuration);
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

		TimeSpan weeklyHours = TimeSpan.FromHours(37.5); // TODO: Use adjustable time value
		TimeSpan fridayHours = weeklyHours - fourDayDuration;

		TimeSpan feierAbend = (TimeSpan)comeFri.Time! + fridayHours - smallBreak;

		TimeSpan miniumumFriday = new TimeSpan(12, 0, 0);
		if (feierAbend < miniumumFriday)
		{
			feierAbend = miniumumFriday;
		}

        feierabendTime.Text = $"{(int)feierAbend.TotalHours}:{feierAbend.Minutes}";
	}
}

// TODO: Remove .NET Intro animation
// TODO: Black line below timepicker
