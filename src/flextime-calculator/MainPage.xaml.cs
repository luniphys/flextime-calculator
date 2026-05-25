using flextime_calculator.Constants;
using System.Linq.Expressions;

namespace flextime_calculator;

public partial class MainPage : ContentPage
{
	private bool _settingsOpen = false;
    private bool _weekMode = true;
    private bool _isLoadingSettings = true;



    public MainPage()
	{
		InitializeComponent();
	}

    
    /// <summary>
    /// Called when the mainpage appears. Displays the first-time setup page if setup has not been completed yet. Otherwise
    /// it loads settings.
    /// </summary>
    protected override async void OnAppearing() // ContentPage Lifecycle: Constructor -> OnAppearing -> OnDisappearing -> Destructor
    {
        base.OnAppearing();

        //Preferences.Clear();

        if (!Preferences.ContainsKey(PreferenceKeys.SetupComplete))
        {
            await Navigation.PushModalAsync(new FirstTimeSetupPage(), animated: true);
            return;
        }

        LoadSettings();
    }



    #region Event handlers

    /// <summary>
    /// The settings button toggles the settings panel with a slide animation.
    /// </summary>
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


    /// <summary>
    /// Handles the swipe gesture to close the settings panel.
    /// </summary>
    private void CloseSettings_Swiped(object sender, SwipedEventArgs e)
    {
        if (_settingsOpen)
        {
            SettingsButton_Clicked(sender, e);
        }
    }


    /// <summary>
    /// Handles the swipe gesture to open the settings panel.
    /// </summary>
    private void OpenSettings_Swiped(object sender, SwipedEventArgs e)
    {
        if (!_settingsOpen)
        {
            SettingsButton_Clicked(sender, e);
        }
    }


    /// <summary>
    /// The switch button is toggling between week and day view modes.
    /// </summary>
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


    /// <summary>
    /// Recalculates Feierabend when a TimePicker property changes.
    /// </summary>
    private void TimePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_isLoadingSettings) { return; }

        if (sender is TimePicker)
		{
            SaveCurrentState();
            CalculateFeierabend();
        }
    }


    /// <summary>
    /// Sets come and go times when a TimePicker property from settings changes. Then recalculates Feierabend.
    /// </summary>
    private void UsualTimePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_isLoadingSettings) { return; }

        if (sender is TimePicker)
        {
            var uComeTime = usualComeTime.Time;
            var uGoTime = usualGoTime.Time;

			comeMon.Time = uComeTime;
            comeTue.Time = uComeTime;
            comeWed.Time = uComeTime;
            comeThu.Time = uComeTime;
            comeFri.Time = uComeTime;

            goMon.Time = uGoTime;
            goTue.Time = uGoTime;
            goWed.Time = uGoTime;
            goThu.Time = uGoTime;

            comeDay.Time = uComeTime;

            CalculateFeierabend();
        }
    }


    /// <summary>
    /// Clicking Entry selects all text.
    /// </summary>
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


    /// <summary>
    /// Restore default button restores usual come and go times by triggering the usual time picker property changed handler.
    /// </summary>
    private void RestoreButton_Clicked(object sender, EventArgs e)
    {
        UsualTimePicker_PropertyChanged(usualComeTime, new System.ComponentModel.PropertyChangedEventArgs(nameof(TimePicker.Time)));
    }

    #endregion



    #region Private (helper) methods

    /// <summary>
    /// Loads work/break times from storage and sets them as values.
    /// </summary>
    private void LoadSettings()
    {
        _isLoadingSettings = true;

        // Settings
        TrySetTime(usualComeTime, PreferenceKeys.UsualComeTime, "06:00");
        TrySetTime(usualGoTime, PreferenceKeys.UsualGoTime, "14:15");

        weeklyHours.Text = Preferences.Get(PreferenceKeys.WeeklyHours, "37");
        weeklyMinutes.Text = Preferences.Get(PreferenceKeys.WeeklyMinutes, "30");

        double weeklyTime = TimeToDouble(weeklyHours.Text, weeklyMinutes.Text);
        dailyHours.Text = (Math.Floor((weeklyTime / 5))).ToString();
        dailyMinutes.Text = (Math.Round(((weeklyTime / 5) % 1) * 60)).ToString();

        smallBreak.Text = Preferences.Get(PreferenceKeys.SmallBreak, "15");
        mainBreak.Text = Preferences.Get(PreferenceKeys.MainBreak, "30");

        // Come and go times
        string defaultCome = usualComeTime.Time.ToString()!;
        string defaultGo = usualGoTime.Time.ToString()!;

        TrySetTime(comeMon, PreferenceKeys.ComeMon, defaultCome);
        TrySetTime(comeTue, PreferenceKeys.ComeTue, defaultCome);
        TrySetTime(comeWed, PreferenceKeys.ComeWed, defaultCome);
        TrySetTime(comeThu, PreferenceKeys.ComeThu, defaultCome);
        TrySetTime(comeFri, PreferenceKeys.ComeFri, defaultCome);
        TrySetTime(goMon, PreferenceKeys.GoMon, defaultGo);
        TrySetTime(goTue, PreferenceKeys.GoTue, defaultGo);
        TrySetTime(goWed, PreferenceKeys.GoWed, defaultGo);
        TrySetTime(goThu, PreferenceKeys.GoThu, defaultGo);

        TrySetTime(comeDay, PreferenceKeys.ComeDay, defaultCome);

        _isLoadingSettings = false;

        CalculateFeierabend();
    }


    /// <summary>
    /// Attempts to set the time picker's time from a stored preference value.
    /// </summary>
    /// <param name="picker">The time picker to update.</param>
    /// <param name="key">The preference key to retrieve.</param>
    /// <param name="defaultValue">The default value to use if the key is not found.</param>
    private static void TrySetTime(TimePicker picker, string key, string defaultValue)
    {
        if (TimeSpan.TryParse(Preferences.Get(key, defaultValue), out var time))
        {
            picker.Time = time;
        }
    }


    /// <summary>
    /// Saves the current come and go times to application preferences.
    /// </summary>
    private void SaveCurrentState()
    {
        Preferences.Set(PreferenceKeys.ComeMon, comeMon.Time.ToString());
        Preferences.Set(PreferenceKeys.ComeTue, comeTue.Time.ToString());
        Preferences.Set(PreferenceKeys.ComeWed, comeWed.Time.ToString());
        Preferences.Set(PreferenceKeys.ComeThu, comeThu.Time.ToString());
        Preferences.Set(PreferenceKeys.ComeFri, comeFri.Time.ToString());
        Preferences.Set(PreferenceKeys.GoMon, goMon.Time.ToString());
        Preferences.Set(PreferenceKeys.GoTue, goTue.Time.ToString());
        Preferences.Set(PreferenceKeys.GoWed, goWed.Time.ToString());
        Preferences.Set(PreferenceKeys.GoThu, goThu.Time.ToString());
    }


    /// <summary>
    /// Calculates the end-of-work time (german: 'Feierabend') for Friday and a single day based on weekly and daily hours, break durations,
    /// and hours worked Monday through Thursday. It also evaluates the time differences (deltas) between needed and actual working time.
    /// </summary>
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
        TimeSpan cumDelta = TimeSpan.Zero;

        for (int i = 0; i < 4; i++)
        {
            deltaTime = durations[i] - totalDailyHours;
            PrintDelta(dayList[i].Item1, deltaTime);

            cumDelta += deltaTime;
            PrintDelta(dayList[i].Item2, cumDelta);
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

        TimeSpan minimumFriday = new TimeSpan(12, 0, 0); // Can't leave before 12:00
		if (feierAbendWeek < minimumFriday)
		{
            feierAbendWeek = minimumFriday;
		}
        feierabendTimeWeek.Text = $"{feierAbendWeek.Hours}:{feierAbendWeek.Minutes:D2}";


        // Calculating Feierabend day
        TimeSpan feierAbendDay = (TimeSpan)comeDay.Time! + totalDailyHours + totalBreak;
        feierabendTimeDay.Text = $"{feierAbendDay.Hours}:{feierAbendDay.Minutes:D2}";
    }


    /// <summary>
    /// Converts time from string hours and minutes to a decimal hour representation.
    /// </summary>
    /// <returns>The time as a decimal value in hours, where minutes are converted to a fraction of an hour.</returns>
    /// <remarks>Returns 0.0 if the input is invalid or if hours/minutes are negative or minutes exceed 60.</remarks>
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


    /// <summary>
    /// Formats and displays a time delta in a label with sign indicators.
    /// </summary>
    /// <param name="label">The label which Text is to update with the formatted time.</param>
    /// <param name="time">The TimeSpan delta to format.</param>
    private void PrintDelta(Label label, TimeSpan time)
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

    #endregion
}

// TODO: Opening settingspanel enters usualtimes from firstsetuppage every time.
