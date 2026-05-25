namespace flextime_calculator;

public partial class MainPage : ContentPage
{
	private bool _settingsOpen = false;
    private bool _weekMode = true;


	public MainPage()
	{
		InitializeComponent();
	}


    protected override async void OnAppearing()
    {
        base.OnAppearing();

        //Preferences.Remove("setupComplete");
        if (!Preferences.ContainsKey("setupComplete"))
        {
            await Navigation.PushModalAsync(new FirstTimeSetupPage(), animated: true);
        }
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


    private void CloseSettings_Swiped(object sender, SwipedEventArgs e)
    {
        if (_settingsOpen)
        {
            SettingsButton_Clicked(sender, e);
        }
    }


    private void OpenSettings_Swiped(object sender, SwipedEventArgs e)
    {
        if (!_settingsOpen)
        {
            SettingsButton_Clicked(sender, e);
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

            comeDay.Time = (TimeSpan)usualComeTime.Time!;

            CalculateFeierabend();
        }
    }


    private void Entry_Focused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), selectAll);

            void selectAll()
            {
                entry.CursorPosition = 0;
                entry.SelectionLength = entry.Text?.Length ?? 0; // x = y ?? z -> x = y if y is not null. If y is null then x = z
            }
        }
    }



    private void CalculateFeierabend()
	{
        double smallBreakDouble = TimeToDouble("0", smallBreak.Text);
		TimeSpan smallBreakTS = TimeSpan.FromHours(smallBreakDouble);
        double mainBreakDouble = TimeToDouble("0", mainBreak.Text);
        TimeSpan mainBreakTS = TimeSpan.FromHours(mainBreakDouble);
		TimeSpan totalBreak = smallBreakTS + mainBreakTS;

        double weeklyTotal = TimeToDouble(weeklyHours.Text, weeklyMinutes.Text);
        TimeSpan totalWeeklyHours = TimeSpan.FromHours(weeklyTotal);

        double dailyTotal = TimeToDouble(dailyHours.Text, dailyMinutes.Text);
        TimeSpan totalDailyHours = TimeSpan.FromHours(dailyTotal);

        TimeSpan monDuration = (TimeSpan)goMon.Time! - (TimeSpan)comeMon.Time! - totalBreak;
        TimeSpan tueDuration = (TimeSpan)goTue.Time! - (TimeSpan)comeTue.Time! - totalBreak;
        TimeSpan wedDuration = (TimeSpan)goWed.Time! - (TimeSpan)comeWed.Time! - totalBreak;
        TimeSpan thuDuration = (TimeSpan)goThu.Time! - (TimeSpan)comeThu.Time! - totalBreak;


        // Updating delta times
        List<TimeSpan> durations = new List<TimeSpan> { monDuration, tueDuration, wedDuration, thuDuration };
        List<(Label, Label)> dayList = new List<(Label, Label)> { (dayDeltaMon, cumDeltaMon), (dayDeltaTue, cumDeltaTue), (dayDeltaWed, cumDeltaWed), (dayDeltaThu, cumDeltaThu) };

        TimeSpan deltaTime;
        TimeSpan cumDelta = new TimeSpan(0, 0, 0);

        for (int i = 0; i < 4; i++)
        {
            deltaTime = durations[i] - totalDailyHours;
            printDelta(dayList[i].Item1, deltaTime);

            cumDelta += deltaTime;
            printDelta(dayList[i].Item2, cumDelta);
        }
    

        // Calculating Feierabend Week
        TimeSpan fourDayDuration = monDuration + tueDuration + wedDuration + thuDuration;
		TimeSpan fridayHours = totalWeeklyHours - fourDayDuration;
		TimeSpan feierAbendWeek = (TimeSpan)comeFri.Time! + fridayHours + smallBreakTS;

        TimeSpan OneOClock = new TimeSpan(13, 0, 0); // Leaving before 13:00 won't add the main break to working time
        if (feierAbendWeek > OneOClock)
        {
            feierAbendWeek += mainBreakTS;
        }

        TimeSpan miniumumFriday = new TimeSpan(12, 0, 0); // Can't leave before 12:00
		if (feierAbendWeek < miniumumFriday)
		{
            feierAbendWeek = miniumumFriday;
		}
        printFeierabend(feierabendTimeWeek, feierAbendWeek);


        // Calculating Feierabend day
        TimeSpan feierAbendDay = (TimeSpan)comeDay.Time! + totalDailyHours + totalBreak;
        printFeierabend(feierabendTimeDay, feierAbendDay);
    }


    private double TimeToDouble(string hours, string minutes)
    {
        double hoursDouble = 0.0;
        double minutesDouble = 0.0;

        if (double.TryParse(hours, out double parsedHours))
        {
            hoursDouble = parsedHours;
        }

        if (double.TryParse(minutes, out double parsedMinutes))
        {
            minutesDouble = parsedMinutes;
        }

        if (hoursDouble < 0 || minutesDouble < 0 || minutesDouble > 60)
        {
            return 0.0;
        }

        return hoursDouble + minutesDouble / 60;
    }


    private void printDelta(Label label, TimeSpan time)
    {
        double hours = time.Hours;
        double minutes = time.Minutes;

        if (Math.Abs(hours) > 0)
        {
            if (hours > 0)
            {
                label.Text = $"+{hours}h {Math.Abs(minutes)}min";
            }
            else
            {
                label.Text = $"{hours}h {Math.Abs(minutes)}min";
            }
        }
        else
        {
            if (minutes > 0)
            {
                label.Text = $"+{minutes}min";
            }
            else
            {
                label.Text = $"{minutes}min";
            }
        }
    }


    private void printFeierabend(Label label, TimeSpan feierabend)
    {
        if (feierabend.Minutes < 10)
        {
            label.Text = $"{feierabend.Hours}:0{feierabend.Minutes}";
        }
        else
        {
            label.Text = $"{feierabend.Hours}:{feierabend.Minutes}";
        }
    }
}

// TODO: First time starting app: Enter setting times.
