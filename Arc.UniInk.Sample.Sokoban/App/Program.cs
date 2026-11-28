using System;
using Avalonia;

namespace Arc.UniInk.Sample.Sokoban.App
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // --list / --solve / --play / --script run headless , so an agent can play without the window
            if (ConsolePlayer.TryRun(args)) return;

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
    }
}
