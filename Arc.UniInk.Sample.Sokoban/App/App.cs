using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;

namespace Arc.UniInk.Sample.Sokoban.App
{
    public sealed class App : Application
    {
        public override void Initialize()
        {
            Styles.Add(new FluentTheme());
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = new MainWindow();

                desktop.MainWindow = window;

                // --replay : the window plays the script by itself , typing each line and clearing it again
                if (Program.ReplayScript is { Length: > 0 })
                {
                    window.DemoLoadLevel(Program.ReplayLevel);
                    new Replayer(window, Program.ReplayScript).Start();
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
