﻿using Android.App;
using Android.Content.PM;

using Avalonia;
using Avalonia.Android;

namespace Tetris.Android;

[Activity(
    Label = "Tetris.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/tetris",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}
