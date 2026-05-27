# src/flextime-calculator

Main application sourcecode directory.

## Contents

- `App.xaml` / `App.xaml.cs`: Application entry point
- `AppShell.xaml` / `AppShell.xaml.cs`: Defining the app's pages
- `MainPage.xaml` / `MainPage.xaml.cs`: Main screen with week and day views and settings panel
- `FirstTimeSetupPage.xaml` / `FirstTimeSetupPage.xaml.cs`: Multi-step setup shown on first launch
- `MauiProgram.cs`: App configuration
- `flextime-calculator.csproj`: Project file with MAUI settings
- `Constants/`: Preference keys for persistent storage
- `Platforms/`: Platform-specific entry points
- `Resources/`: App icon, splash screen, fonts, images, and XAML style/color resources
- `Properties/`: Launch settings

## Notes

- All user settings and time states are stored via MAUI's `Preferences`
