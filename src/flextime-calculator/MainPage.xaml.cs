using flextime_calculator.Constants;

namespace flextime_calculator;

public partial class MainPage : ContentPage
{
	private bool _settingsOpen = false;
    private bool _weekMode = true;
    private bool _isLoadingSettings = true;
    private bool _infoTextOpen = false;
    private bool _lateShift = false;
    private Color _defaultPurple = Color.FromRgb(80, 43, 212);


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
            switchButton.IsEnabled = true;
            settingsPanel.Animate("close", v => settingsPanel.WidthRequest = v, start: this.Width * panelWidth, end: 0, length: animationDuration, finished: (v, c) => settingsPanel.IsVisible = false);
        }
		else
		{
            _settingsOpen = true;
            dimOverlay.IsVisible = true;
            settingsPanel.IsVisible = true;
            mainPageGrid.IsEnabled = false;
            dayPageGrid.IsEnabled = false;
            switchButton.IsEnabled = false;
            switchButton.BackgroundColor = _defaultPurple;
            settingsPanel.Animate("open", v => settingsPanel.WidthRequest = v, start: 0, end: this.Width * panelWidth, length: animationDuration);
        }

        if (_infoTextOpen)
        {
            feierabendInfoWeek.IsVisible = false;
            feierabendInfoDay.IsVisible = false;
            _infoTextOpen = false;
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
            switchButton.Text = "Woche";
            switchButton.FontSize = GetNamedFontSize("Caption");
            mainPageGrid.IsEnabled = false;
            mainPageGrid.IsVisible = false;
            _weekMode = false;

            dayPageGrid.IsEnabled = true;
            dayPageGrid.IsVisible = true;
        }
        else
        {
            switchButton.Text = "Tag";
            switchButton.FontSize = GetNamedFontSize("Medium");
            mainPageGrid.IsEnabled = true;
            mainPageGrid.IsVisible = true;
            _weekMode = true;

            dayPageGrid.IsEnabled = false;
            dayPageGrid.IsVisible = false;
        }

        if (_infoTextOpen)
        {
            feierabendInfoWeek.IsVisible = false;
            feierabendInfoDay.IsVisible = false;
            dimOverlay.IsVisible = false;
            _infoTextOpen = false;
        }
    }


    /// <summary>
    /// Recalculates Feierabend and saves current state when a TimePicker property changes.
    /// </summary>
    private void TimePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_isLoadingSettings) { return; }
        if (e.PropertyName != nameof(TimePicker.Time)) { return; }

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
        if (e.PropertyName != nameof(TimePicker.Time)) { return; }

        if (sender is TimePicker picker)
        {
            _isLoadingSettings = true;

            var uComeTime = usualComeTime.Time;
            var uGoTime = usualGoTime.Time;

            if (picker == usualComeTime)
            {
                comeMon.Time = uComeTime;
                comeTue.Time = uComeTime;
                comeWed.Time = uComeTime;
                comeThu.Time = uComeTime;
                comeFri.Time = uComeTime;

                comeDay.Time = uComeTime;
            }

            if (picker == usualGoTime)
            {
                goMon.Time = uGoTime;
                goTue.Time = uGoTime;
                goWed.Time = uGoTime;
                goThu.Time = uGoTime;
            }

            _isLoadingSettings = false;

            SaveCurrentState();
            CalculateFeierabend();
        }
    }


    /// <summary>
    /// When Timepickers watching break times change, state is saved and Feierabend recalculated.
    /// </summary>
    private void BreakTimePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_isLoadingSettings) { return; }
        if (e.PropertyName != nameof(TimePicker.Time)) { return; }

        if (sender is TimePicker)
        {
            SaveCurrentState();
            CalculateFeierabend();
        }
    }


    /// <summary>
    /// Recalculates Feierabend and saves current state when a change in text of an entry happens.
    /// </summary>
    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isLoadingSettings) { return; }

        if (sender is Entry)
        {
            SaveCurrentState();
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
        if (sender is Button)
        {
            UsualTimePicker_PropertyChanged(usualComeTime, new System.ComponentModel.PropertyChangedEventArgs(nameof(TimePicker.Time)));
            UsualTimePicker_PropertyChanged(usualGoTime, new System.ComponentModel.PropertyChangedEventArgs(nameof(TimePicker.Time)));
        }
    }


    /// <summary>
    /// Info button opens text bubble giving information about Feierabend rules for the week.
    /// </summary>
    private void QuestionMarkWeekButton_Clicked(object sender, EventArgs e)
    {
        if (sender is Button)
        {
            if (!_infoTextOpen)
            {
                if (_lateShift)
                {
                    bulletPoint1.Text = "Man kann nicht vor 16:15 Uhr gehen.";
                    bulletPoint2.Text = "Kommt man nach 9:00 Uhr wird die kleine Pause nicht angerechnet.";
                }

                else
                {
                    bulletPoint1.Text = "Man kann nicht vor 12:00 Uhr gehen.";
                    bulletPoint2.Text = "Geht man vor 13:00 Uhr wird die Mittagspause nicht angerechnet.";
                }

                feierabendInfoWeek.IsVisible = true;
                dimOverlay.IsVisible = true;
                _infoTextOpen = true;
            }
        }
    }


    /// <summary>
    /// Info button opens text bubble giving information about Feierabend rules for the day.
    /// </summary>
    private void QuestionMarkDayButton_Clicked(object sender, EventArgs e)
    {
        if (sender is Button)
        {
            if (!_infoTextOpen)
            {
                feierabendInfoDay.IsVisible = true;
                dimOverlay.IsVisible = true;
                _infoTextOpen = true;
            }
        }
    }


    /// <summary>
    /// Tapping anywhere on screen to make info text bubble disappear.
    /// </summary>
    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        if (_infoTextOpen)
        {
            feierabendInfoWeek.IsVisible = false;
            feierabendInfoDay.IsVisible = false;
            dimOverlay.IsVisible = false;
            _infoTextOpen = false;
        }
    }


    /// <summary>
    /// Sets lateShift boolean based on CheckBox is checked or not.
    /// </summary>
    private void LateShiftCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_isLoadingSettings) { return; }

        if (sender is CheckBox)
        {
            _lateShift = lateShiftCheckBox.IsChecked;

            SaveCurrentState();
            CalculateFeierabend();
        }
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
        lateShiftCheckBox.IsChecked = Preferences.Get(PreferenceKeys.LateShift, false);

        TrySetTime(usualComeTime, PreferenceKeys.UsualComeTime, "06:00");
        TrySetTime(usualGoTime, PreferenceKeys.UsualGoTime, "14:15");

        weeklyHours.Text = Preferences.Get(PreferenceKeys.WeeklyHours, "37");
        weeklyMinutes.Text = Preferences.Get(PreferenceKeys.WeeklyMinutes, "30");

        double weeklyTimeDouble = TimeToDouble(weeklyHours.Text, weeklyMinutes.Text);
        double dailyHoursDouble = Math.Floor((weeklyTimeDouble / 5));
        double dailyMinutesDouble = Math.Round(((weeklyTimeDouble / 5) % 1) * 60);
        dailyHours.Text = Preferences.Get(PreferenceKeys.DailyHours, dailyHoursDouble.ToString());
        dailyMinutes.Text = Preferences.Get(PreferenceKeys.DailyMinutes, dailyMinutesDouble.ToString());

        TrySetTime(smallBreakStart, PreferenceKeys.SmallBreakStart, "09:00");
        TrySetTime(smallBreakEnd, PreferenceKeys.SmallBreakEnd, "09:15");
        TrySetTime(mainBreakStart, PreferenceKeys.MainBreakStart, "12:00");
        TrySetTime(mainBreakEnd, PreferenceKeys.MainBreakEnd, "12:30");

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

        _lateShift = lateShiftCheckBox.IsChecked;

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

        Preferences.Set(PreferenceKeys.UsualComeTime, usualComeTime.Time.ToString());
        Preferences.Set(PreferenceKeys.UsualGoTime, usualGoTime.Time.ToString());
        Preferences.Set(PreferenceKeys.WeeklyHours, weeklyHours.Text);
        Preferences.Set(PreferenceKeys.WeeklyMinutes, weeklyMinutes.Text);
        Preferences.Set(PreferenceKeys.DailyHours, dailyHours.Text);
        Preferences.Set(PreferenceKeys.DailyMinutes, dailyMinutes.Text);
        Preferences.Set(PreferenceKeys.SmallBreakStart, smallBreakStart.Time.ToString());
        Preferences.Set(PreferenceKeys.SmallBreakEnd, smallBreakEnd.Time.ToString());
        Preferences.Set(PreferenceKeys.MainBreakStart, mainBreakStart.Time.ToString());
        Preferences.Set(PreferenceKeys.MainBreakEnd, mainBreakEnd.Time.ToString());

        Preferences.Set(PreferenceKeys.ComeDay, comeDay.Time.ToString());

        Preferences.Set(PreferenceKeys.LateShift, lateShiftCheckBox.IsChecked);
    }


    /// <summary>
    /// Calculates the end-of-work time (german: 'Feierabend') for Friday and a single day based on weekly and daily hours, break durations,
    /// and hours worked Monday through Thursday. It also evaluates the time differences (deltas) between needed and actual working time.
    /// </summary>
    private void CalculateFeierabend()
	{
        // Initialization
        TimeSpan smallBreakStartTS = (TimeSpan)smallBreakStart.Time!;
        TimeSpan smallBreakEndTS = (TimeSpan)smallBreakEnd.Time!;
        TimeSpan smallBreakDuration = smallBreakEndTS - smallBreakStartTS;

        TimeSpan mainBreakStartTS = (TimeSpan)mainBreakStart.Time!;
        TimeSpan mainBreakEndTS = (TimeSpan)mainBreakEnd.Time!;
        TimeSpan mainBreakDuration = mainBreakEndTS - mainBreakStartTS;

        if (smallBreakDuration < TimeSpan.Zero || mainBreakDuration < TimeSpan.Zero)
        {
            feierabendTimeWeek.Text = "Invalid break time.";
            feierabendTimeDay.Text = "Invalid break time.";
            return;
        }

        TimeSpan totalBreakDuration = smallBreakDuration + mainBreakDuration;
        
        double weeklyTotal = TimeToDouble(weeklyHours.Text, weeklyMinutes.Text);
        TimeSpan totalWeeklyHours = TimeSpan.FromHours(weeklyTotal);

        double dailyTotal = TimeToDouble(dailyHours.Text, dailyMinutes.Text);
        TimeSpan totalDailyHours = TimeSpan.FromHours(dailyTotal);


        // Come/Go & Duration Lists
        List<TimeSpan> comeTimes = new List<TimeSpan> { (TimeSpan)comeMon.Time!, (TimeSpan)comeTue.Time!, (TimeSpan)comeWed.Time!, (TimeSpan)comeThu.Time!, (TimeSpan)comeFri.Time!, (TimeSpan)comeDay.Time! };
        List<TimeSpan> goTimes = new List<TimeSpan> { (TimeSpan)goMon.Time!, (TimeSpan)goTue.Time!, (TimeSpan)goWed.Time!, (TimeSpan)goThu.Time! };

        List<TimeSpan> durations = new List<TimeSpan> { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero };


        // Check if small break needs to be subtracted
        for (int i = 0; i < comeTimes.Count; i++)
        {
            comeTimes[i] = (comeTimes[i] > smallBreakStartTS && comeTimes[i] <= smallBreakEndTS) ? smallBreakEndTS : comeTimes[i];
        }

        for (int i = 0; i < durations.Count; i++)
        {
            durations[i] = (comeTimes[i] >= smallBreakEndTS) ? (goTimes[i] - comeTimes[i] - mainBreakDuration) : (goTimes[i] - comeTimes[i] - totalBreakDuration);
        }


        // Updating delta times
        List<(Label dayDelta, Label cumDelta)> dayList = new List<(Label, Label)> { (dayDeltaMon, cumDeltaMon), (dayDeltaTue, cumDeltaTue), (dayDeltaWed, cumDeltaWed), (dayDeltaThu, cumDeltaThu) };

        TimeSpan deltaTime;
        TimeSpan cumDelta = TimeSpan.Zero;

        for (int i = 0; i < 4; i++)
        {
            deltaTime = durations[i] - totalDailyHours;
            UpdateDeltaLabel(dayList[i].dayDelta, deltaTime);

            cumDelta += deltaTime;
            UpdateDeltaLabel(dayList[i].cumDelta, cumDelta);
        }
    

        // Calculating Feierabend Week
        TimeSpan fourDayDuration = TimeSpan.Zero;
        foreach (TimeSpan duration in durations)
        {
            fourDayDuration += duration;
        }
		TimeSpan fridayHours = totalWeeklyHours - fourDayDuration;
		TimeSpan feierAbendWeek = (comeTimes[4] >= smallBreakEndTS) ? comeTimes[4] + fridayHours : comeTimes[4] + fridayHours + smallBreakDuration;

        TimeSpan OneOClock = new TimeSpan(13, 0, 0); // Leaving before 13:00 won't add the main break to working times
        feierAbendWeek = (feierAbendWeek < OneOClock) ? feierAbendWeek : feierAbendWeek + mainBreakDuration;

        TimeSpan TwelveOClock = new TimeSpan(12, 0, 0); // Can't leave before 12:00
        if (!_lateShift)
        {
            feierAbendWeek = (feierAbendWeek < TwelveOClock) ? TwelveOClock : feierAbendWeek;
        }

        TimeSpan FourFifteen = new TimeSpan(16, 15, 0); // Can't leave before 16:15 at late shift
        if (_lateShift)
        {
            feierAbendWeek = (feierAbendWeek < FourFifteen) ? FourFifteen : feierAbendWeek;
        }

        feierabendTimeWeek.Text = $"{feierAbendWeek.Hours}:{feierAbendWeek.Minutes:D2}";


        // Calculating Feierabend day
        TimeSpan feierAbendDay = (comeTimes[5] >= smallBreakEndTS) ? comeTimes[5] + totalDailyHours + mainBreakDuration : comeTimes[5] + totalDailyHours + totalBreakDuration;

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
    private void UpdateDeltaLabel(Label label, TimeSpan time)
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


    /// <summary>
    /// Gets double representation of named FontSize as string input.
    /// </summary>
    private double GetNamedFontSize(string namedSize)
    {
        var converter = new FontSizeConverter();
        return (double)converter.ConvertFromInvariantString(namedSize)!;
    }

    #endregion

}
