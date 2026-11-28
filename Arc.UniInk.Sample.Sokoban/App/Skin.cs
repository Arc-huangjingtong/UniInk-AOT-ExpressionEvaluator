using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace Arc.UniInk.Sample.Sokoban.App
{
    /// <summary> One place for the paints , the spacing and the control styles of the window </summary>
    public static class Skin
    {
        public static readonly Color Backdrop   = Color.Parse("#0E1014");
        public static readonly Color Surface    = Color.Parse("#161A21");
        public static readonly Color SurfaceAlt = Color.Parse("#1C212B");
        public static readonly Color Line       = Color.Parse("#262C38");
        public static readonly Color Ink        = Color.Parse("#E8EBF2");
        public static readonly Color Muted      = Color.Parse("#8B94A7");
        public static readonly Color Accent     = Color.Parse("#4C8DFF");
        public static readonly Color Success    = Color.Parse("#57C15F");
        public static readonly Color Danger     = Color.Parse("#FF6B6B");

        public static readonly IBrush BackdropBrush   = new SolidColorBrush(Backdrop);
        public static readonly IBrush SurfaceBrush    = new SolidColorBrush(Surface);
        public static readonly IBrush SurfaceAltBrush = new SolidColorBrush(SurfaceAlt);
        public static readonly IBrush LineBrush       = new SolidColorBrush(Line);
        public static readonly IBrush InkBrush        = new SolidColorBrush(Ink);
        public static readonly IBrush MutedBrush      = new SolidColorBrush(Muted);
        public static readonly IBrush AccentBrush     = new SolidColorBrush(Accent);
        public static readonly IBrush SuccessBrush    = new SolidColorBrush(Success);
        public static readonly IBrush DangerBrush     = new SolidColorBrush(Danger);

        public static readonly IBrush BoardBrush      = new SolidColorBrush(Color.Parse("#10131A"));
        public static readonly IBrush FloorBrush      = new SolidColorBrush(Color.Parse("#1A1E28"));
        public static readonly IBrush WallBrush       = new SolidColorBrush(Color.Parse("#2C3346"));
        public static readonly IBrush WallTopBrush    = new SolidColorBrush(Color.Parse("#3A4359"));
        public static readonly IBrush GoalRingBrush   = new SolidColorBrush(Color.Parse("#57C15F"), 0.55);
        public static readonly IBrush BoxBrush        = new SolidColorBrush(Color.Parse("#C08A4E"));
        public static readonly IBrush BoxTopBrush     = new SolidColorBrush(Color.Parse("#D9A76A"));
        public static readonly IBrush BoxDoneBrush    = new SolidColorBrush(Color.Parse("#57C15F"));
        public static readonly IBrush BoxDoneTopBrush = new SolidColorBrush(Color.Parse("#7BD482"));
        public static readonly IBrush PlayerBrush     = new SolidColorBrush(Color.Parse("#4C8DFF"));
        public static readonly IBrush PlayerGlint     = new SolidColorBrush(Color.Parse("#A9C9FF"));

        public static readonly BoxShadows CardShadow = BoxShadows.Parse("0 18 40 0 #66000000");
        public static readonly BoxShadows SoftShadow = BoxShadows.Parse("0 6 18 0 #40000000");

        public static readonly FontFamily Mono = FontFamily.Parse("Menlo, Consolas, monospace");

        private static readonly Color Hover    = Color.Parse("#242A36");
        private static readonly Color Pressed  = Color.Parse("#2E3546");
        private static readonly Color HoverAcc = Color.Parse("#6BA0FF");
        private static readonly Color PressAcc = Color.Parse("#3B78E0");

        /// <summary> The control styles : one radius , one border , one hover rule </summary>
        public static Styles Build()
        {
            var styles = new Styles();

            styles.Add(new Style(x => x.OfType<Button>())
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, SurfaceAltBrush),
                    new Setter(TemplatedControl.BorderBrushProperty, LineBrush),
                    new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(1.0)),
                    new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(9.0)),
                    new Setter(TemplatedControl.PaddingProperty, new Thickness(15.0, 7.0)),
                    new Setter(TemplatedControl.FontSizeProperty, 12.5),
                    new Setter(TemplatedControl.ForegroundProperty, InkBrush),
                    new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Center)
                }
            });

            styles.Add(new Style(x => x.OfType<Button>().Class(":pointerover"))
            {
                Setters = { new Setter(TemplatedControl.BackgroundProperty, new SolidColorBrush(Hover)) }
            });

            styles.Add(new Style(x => x.OfType<Button>().Class(":pressed"))
            {
                Setters = { new Setter(TemplatedControl.BackgroundProperty, new SolidColorBrush(Pressed)) }
            });

            styles.Add(new Style(x => x.OfType<Button>().Class("primary"))
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, AccentBrush),
                    new Setter(TemplatedControl.BorderBrushProperty, AccentBrush),
                    new Setter(TemplatedControl.ForegroundProperty, Brushes.White),
                    new Setter(TemplatedControl.FontWeightProperty, FontWeight.SemiBold)
                }
            });

            styles.Add(new Style(x => x.OfType<Button>().Class("primary").Class(":pointerover"))
            {
                Setters = { new Setter(TemplatedControl.BackgroundProperty, new SolidColorBrush(HoverAcc)) }
            });

            styles.Add(new Style(x => x.OfType<Button>().Class("primary").Class(":pressed"))
            {
                Setters = { new Setter(TemplatedControl.BackgroundProperty, new SolidColorBrush(PressAcc)) }
            });

            styles.Add(new Style(x => x.OfType<Button>().Class("ghost"))
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                    new Setter(TemplatedControl.BorderBrushProperty, LineBrush),
                    new Setter(TemplatedControl.ForegroundProperty, MutedBrush)
                }
            });

            styles.Add(new Style(x => x.OfType<Button>().Class("ghost").Class(":pointerover"))
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, SurfaceAltBrush),
                    new Setter(TemplatedControl.ForegroundProperty, InkBrush)
                }
            });

            styles.Add(new Style(x => x.OfType<TextBox>())
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, new SolidColorBrush(Color.Parse("#12161D"))),
                    new Setter(TemplatedControl.BorderBrushProperty, LineBrush),
                    new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(1.0)),
                    new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(9.0)),
                    new Setter(TemplatedControl.PaddingProperty, new Thickness(12.0, 10.0)),
                    new Setter(TemplatedControl.FontSizeProperty, 13.0),
                    new Setter(TemplatedControl.ForegroundProperty, InkBrush)
                }
            });

            styles.Add(new Style(x => x.OfType<ComboBox>())
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, SurfaceAltBrush),
                    new Setter(TemplatedControl.BorderBrushProperty, LineBrush),
                    new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(1.0)),
                    new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(9.0)),
                    new Setter(TemplatedControl.FontSizeProperty, 12.5),
                    new Setter(TemplatedControl.ForegroundProperty, InkBrush)
                }
            });

            styles.Add(new Style(x => x.OfType<CheckBox>())
            {
                Setters =
                {
                    new Setter(TemplatedControl.ForegroundProperty, MutedBrush),
                    new Setter(TemplatedControl.FontSizeProperty, 12.5),
                    new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Center)
                }
            });

            return styles;
        }

        /// <summary> A small uppercase caption above a group </summary>
        public static TextBlock Caption(string text) => new()
        {
            Text       = text.ToUpperInvariant(),
            FontSize   = 10.5,
            FontWeight = FontWeight.SemiBold,
            Foreground = MutedBrush,
            Margin     = new Thickness(0, 0, 0, 8)
        };
    }
}
