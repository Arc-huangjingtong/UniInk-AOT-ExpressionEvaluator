using System;
using System.IO;
using System.Linq;
using Avalonia;

namespace Arc.UniInk.Sample.Sokoban.App
{
    public static class Program
    {
        /// <summary> Set by --replay : the window plays the script itself , so a recording needs no typing </summary>
        public static string[] ReplayScript { get; private set; }

        public static int ReplayLevel { get; private set; }

        [STAThread]
        public static void Main(string[] args)
        {
            // --list / --ask / --play / --script / --solve run headless , so an agent can play without a window
            if (ConsolePlayer.TryRun(args)) return;

            if (args.Length > 2 && args[0] == "--replay")
            {
                ReplayLevel  = int.Parse(args[1]);
                ReplayScript = File.ReadAllLines(args[2])
                                   .Where(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("#"))
                                   .ToArray();
            }

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
    }
}
