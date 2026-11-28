using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Arc.UniInk.Sample.Sokoban.Core;

namespace Arc.UniInk.Sample.Sokoban.App
{
    /// <summary> The window : one board , one policy , one evaluation per turn </summary>
    public sealed class MainWindow : Window
    {
        private const double Tile  = 46;
        private const double Gap   = 6;

        private readonly ComboBox  levelBox  = new() { Name = "level", Width = 168 };
        private readonly TextBox   policyBox = new() { Name = "policy", AcceptsReturn = true, MinHeight = 96, FontFamily = Skin.Mono, FontSize = 13, TextWrapping = TextWrapping.Wrap };
        private readonly TextBlock stepsText = new() { FontFamily = Skin.Mono, FontSize = 12, Foreground = Skin.InkBrush };
        private readonly TextBlock pushesText = new() { FontFamily = Skin.Mono, FontSize = 12, Foreground = Skin.InkBrush };
        private readonly TextBlock goalsText  = new() { FontFamily = Skin.Mono, FontSize = 12, Foreground = Skin.InkBrush };
        private readonly TextBlock levelTitle = new() { FontSize = 12.5, Foreground = Skin.MutedBrush, Margin = new Thickness(0, 2, 0, 0), TextTrimming = TextTrimming.CharacterEllipsis, MaxWidth = 560 };
        private readonly TextBlock status     = new() { FontSize = 12.5, FontWeight = FontWeight.Medium, TextWrapping = TextWrapping.Wrap };
        private readonly TextBlock detail     = new() { FontSize = 11.5, Foreground = Skin.MutedBrush, TextWrapping = TextWrapping.Wrap };
        private readonly Canvas    board      = new();
        private readonly CheckBox  wasd       = new() { Content = "WASD" };
        private readonly CheckBox  auto       = new() { Content = "Auto run" };

        private readonly DispatcherTimer timer;
        private readonly DispatcherTimer solutionTimer;
        private readonly DispatcherTimer pulseTimer;
        private readonly List<Pulse>     pulses = new();

        private Game game;
        private List<Direction> solution;
        private int solutionStep;

        public MainWindow()
        {
            Title           = "UniInk Sokoban";
            Width           = 1080;
            Height          = 720;
            MinWidth        = 940;
            MinHeight       = 640;
            Background      = Skin.BackdropBrush;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Styles.AddRange(Skin.Build());

            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(320) };
            timer.Tick += (_, _) => Step();

            solutionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(220) };
            solutionTimer.Tick += (_, _) => PlaySolutionStep();

            // one ripple per box that lands home , and a slow one over the board when the level is cleared
            pulseTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            pulseTimer.Tick += (_, _) => PulseTick();

            for (var i = 0 ; i < Levels.Count ; i++) levelBox.Items.Add($"{i + 1} · {Levels.Get(i).Name}");

            levelBox.SelectedIndex = 0;
            levelBox.SelectionChanged += (_, _) => LoadLevel(levelBox.SelectedIndex);

            Content = BuildLayout();

            policyBox.Text = "if ( BOX_AT( right ) && ! WALL( px + 2 , py ) ) { return right } else { return down }";

            LoadLevel(0);
            KeyDown += OnKeyDown;
        }

        private Control BuildLayout()
        {
            var root = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,*,Auto"),
                Margin         = new Thickness(24, 20, 24, 20),
                RowSpacing     = 18
            };

            var header = BuildHeader();
            var body   = BuildBody();
            var footer = BuildFooter();

            Grid.SetRow(header, 0);
            Grid.SetRow(body, 1);
            Grid.SetRow(footer, 2);

            root.Children.Add(header);
            root.Children.Add(body);
            root.Children.Add(footer);

            return root;
        }

        private Control BuildHeader()
        {
            var title = new StackPanel
            {
                Spacing  = 0,
                Children =
                {
                    new TextBlock { Text = "UniInk · Sokoban", FontSize = 19, FontWeight = FontWeight.SemiBold, Foreground = Skin.InkBrush },
                    levelTitle
                }
            };

            var counters = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing     = 10,
                VerticalAlignment = VerticalAlignment.Center,
                Children    = { Chip("steps", stepsText), Chip("pushes", pushesText), Chip("boxes", goalsText) }
            };

            var right = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing     = 10,
                VerticalAlignment = VerticalAlignment.Center,
                Children    = { counters, levelBox }
            };

            var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };

            Grid.SetColumn(title, 0);
            Grid.SetColumn(right, 1);

            header.Children.Add(title);
            header.Children.Add(right);

            return header;
        }

        /// <summary> What the squares mean , so the board reads without a manual </summary>
        private static Control BuildLegend()
        {
            var goal = Skin.GoalRingBrush;

            var legend = new StackPanel
            {
                Orientation         = Orientation.Horizontal,
                Spacing             = 18,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            legend.Children.Add(Item(Skin.WallBrush, "wall", false));
            legend.Children.Add(Item(Skin.FloorBrush, "floor", false));
            legend.Children.Add(Item(goal, "goal", true));
            legend.Children.Add(Item(Skin.BoxBrush, "box", false));
            legend.Children.Add(Item(Skin.PlayerBrush, "player", false));

            Grid.SetRow(legend, 1);

            return legend;
        }

        private static Control Item(IBrush brush, string label, bool ring)
        {
            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing     = 7,
                Children    =
                {
                    new Border
                    {
                        Width           = 12,
                        Height          = 12,
                        CornerRadius    = new CornerRadius(ring ? 6 : 3),
                        Background      = ring ? Brushes.Transparent : brush,
                        BorderBrush     = brush,
                        BorderThickness = ring ? new Thickness(2.0) : new Thickness(0.0),
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    new TextBlock { Text = label, FontSize = 11, Foreground = Skin.MutedBrush, VerticalAlignment = VerticalAlignment.Center }
                }
            };
        }

        /// <summary> The board functions , in the monospace voice of the language </summary>
        private static Control Functions()
        {
            var grid = new UniformGrid { Columns = 2 };

            foreach (var name in new[] { "WALL( x , y )", "BOX( x , y )", "GOAL( x , y )", "FREE( x , y )", "CAN_MOVE( d )", "BOX_AT( d )", "GOAL_AT( d )", "px , py" })
            {
                grid.Children.Add(new TextBlock
                {
                    Text      = name,
                    FontFamily = Skin.Mono,
                    FontSize  = 11,
                    Foreground = Skin.MutedBrush,
                    Margin    = new Thickness(0, 0, 0, 5)
                });
            }

            return grid;
        }

        private int Home()
        {
            var home = 0;

            foreach (var box in game.Boxes)
            {
                if (game.IsGoal(box.X, box.Y)) home++;
            }

            return home;
        }

        private static Control Chip(string label, TextBlock value)
        {
            return new Border
            {
                Background      = Skin.SurfaceBrush,
                BorderBrush     = Skin.LineBrush,
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(8),
                Padding         = new Thickness(10, 5),
                Child           = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing     = 6,
                    Children    = { new TextBlock { Text = label, FontSize = 10.5, Foreground = Skin.MutedBrush, VerticalAlignment = VerticalAlignment.Center }, value }
                }
            };
        }

        private Control BuildBody()
        {
            var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,400"), ColumnSpacing = 20 };

            var boardCard = new Border
            {
                Background       = Skin.SurfaceBrush,
                BorderBrush      = Skin.LineBrush,
                BorderThickness  = new Thickness(1),
                CornerRadius     = new CornerRadius(16),
                BoxShadow        = Skin.CardShadow,
                Padding          = new Thickness(20),
                Child            = new Grid
                {
                    RowDefinitions = new RowDefinitions("*,Auto"),
                    RowSpacing     = 14,
                    Children =
                    {
                        new Border
                        {
                            Background      = Skin.BoardBrush,
                            BorderBrush     = Skin.LineBrush,
                            BorderThickness = new Thickness(1.0),
                            CornerRadius    = new CornerRadius(12),
                            Padding         = new Thickness(12),
                            Child           = new Viewbox
                            {
                                Stretch             = Stretch.Uniform,
                                MaxWidth            = 640,
                                MaxHeight           = 520,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment   = VerticalAlignment.Center,
                                Child               = board
                            }
                        },
                        BuildLegend()
                    }
                }
            };

            var side = BuildSide();

            Grid.SetColumn(boardCard, 0);
            Grid.SetColumn(side, 1);

            grid.Children.Add(boardCard);
            grid.Children.Add(side);

            return grid;
        }

        private Control BuildSide()
        {
            var examples = new WrapPanel();

            foreach (var example in new[] { "R", "RR", "RL", "UUL", "if ( BOX_AT( right ) )" })
            {
                var text = example;

                var chip = new Button
                {
                    Content  = text,
                    Classes  = { "ghost" },
                    FontFamily = Skin.Mono,
                    FontSize = 11.5,
                    Margin   = new Thickness(0, 0, 6, 6),
                    Padding  = new Thickness(9, 4)
                };

                chip.Click += (_, _) => { policyBox.Text = text; policyBox.Focus(); };

                examples.Children.Add(chip);
            }

            var policy = new StackPanel
            {
                Spacing  = 8,
                Children = { Skin.Caption("Policy"), policyBox, examples }
            };

            var statusCard = new Border
            {
                Background      = Skin.SurfaceAltBrush,
                CornerRadius    = new CornerRadius(10),
                Padding         = new Thickness(12, 10),
                Child           = new StackPanel { Spacing = 3, Children = { status, detail } }
            };

            var sideCard = new Border
            {
                Background      = Skin.SurfaceBrush,
                BorderBrush     = Skin.LineBrush,
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(16),
                BoxShadow       = Skin.SoftShadow,
                Padding         = new Thickness(20),
                Child           = new StackPanel
                {
                    Spacing  = 18,
                    Children =
                    {
                        policy,
                        new StackPanel
                        {
                            Spacing  = 8,
                            Children = { Skin.Caption("Status"), statusCard }
                        },
                        new StackPanel
                        {
                            Spacing  = 8,
                            Children = { Skin.Caption("Board"), Functions() }
                        },
                        new TextBlock
                        {
                            Text         = "One UniInk expression per turn. It returns a direction code ( 0 up , 1 right , 2 down , 3 left ) " +
                                           "or names an action letter, which queues that move.",
                            FontSize     = 11.5,
                            Foreground   = Skin.MutedBrush,
                            TextWrapping = TextWrapping.Wrap
                        }
                    }
                }
            };

            return sideCard;
        }

        private Control BuildFooter()
        {
            var run = new Button { Name = "run", Content = "Run one turn", Classes = { "primary" } };
            var solve = new Button { Name = "solve", Content = "Solve ( BFS )" };
            var reset = new Button { Content = "Reset" };

            run.Click += (_, _) => Step();
            solve.Click += (_, _) => SolveLevel();
            reset.Click += (_, _) => LoadLevel(levelBox.SelectedIndex);

            auto.IsCheckedChanged += (_, _) =>
            {
                if (auto.IsChecked == true) timer.Start();
                else                        timer.Stop();
            };

            var left = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing     = 10,
                VerticalAlignment = VerticalAlignment.Center,
                Children    = { run, auto, solve, wasd, reset }
            };

            var hint = new TextBlock
            {
                Text      = "Space runs a turn · arrows move the player",
                FontSize  = 11,
                Foreground = Skin.MutedBrush,
                VerticalAlignment = VerticalAlignment.Center
            };

            var footer = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };

            Grid.SetColumn(left, 0);
            Grid.SetColumn(hint, 1);

            footer.Children.Add(left);
            footer.Children.Add(hint);

            return footer;
        }

        private void LoadLevel(int index)
        {
            timer.Stop();
            solutionTimer.Stop();
            solution = null;

            game = new Game(Levels.Get(index < 0 ? 0 : index));

            board.Width  = game.Width * Tile + Gap * 2;
            board.Height = game.Height * Tile + Gap * 2;

            levelTitle.Text = game.Level.Hint;
            detail.Text     = "Level " + (index + 1) + " · " + game.Level.Name;

            Render();
            Message("Ready", Skin.MutedBrush);
        }

        private void Step()
        {
            if (game.Solved)
            {
                timer.Stop();
                Message("Solved — reset or pick another level", Skin.SuccessBrush);

                return;
            }

            var scheme = wasd.IsChecked == true ? InputScheme.Wasd : InputScheme.UrDL;
            var turn   = new Protocol(game, scheme).Evaluate(policyBox.Text);

            if (!turn.Ok)
            {
                timer.Stop();
                Message("Policy error", Skin.DangerBrush, turn.Error);

                return;
            }

            var moved = 0;

            foreach (var direction in turn.Moves)
            {
                var before = HomeCells();

                if (!game.Move(direction)) break;

                moved++;

                foreach (var (x, y) in HomeCells())
                {
                    if (!before.Contains((x, y))) Ripple(x, y);
                }
            }

            Render();

            if (moved < turn.Moves.Count)
            {
                timer.Stop();
                Message($"Blocked after {moved} of {turn.Moves.Count} move(s)", Skin.DangerBrush, $"{turn.Moves[moved]} is not possible here");

                return;
            }

            var source = turn.FromAction ? "action letter" : "direction code";

            if (game.Solved)
            {
                var delay = 0.0;

                foreach (var (x, y) in HomeCells())
                {
                    Ripple(x, y, delay, Skin.SuccessBrush);
                    delay += 90;
                }

                Render();
                Message("Solved!", Skin.SuccessBrush, $"{game.Steps} steps · {game.Pushes} pushes");
            }
            else
            {
                Message($"{turn.Moves.Count} move(s) played", Skin.InkBrush, $"{source} · {game.Steps} steps · {game.Pushes} pushes");
            }
        }

        private void SolveLevel()
        {
            if (game.Solved) return;

            timer.Stop();
            solution     = Solver.Solve(game);
            solutionStep = 0;

            if (solution is null || solution.Count == 0)
            {
                Message("No solution found", Skin.DangerBrush);

                return;
            }

            Message("Solving…", Skin.AccentBrush, $"{solution.Count} moves");

            solutionTimer.Start();
        }

        private void PlaySolutionStep()
        {
            if (solution is null || solutionStep >= solution.Count || game.Solved)
            {
                solutionTimer.Stop();
                Message(game.Solved ? "Solved by BFS" : "The solution ran out", game.Solved ? Skin.SuccessBrush : Skin.DangerBrush);

                return;
            }

            game.Move(solution[solutionStep++]);
            Render();
            Message("Solving…", Skin.AccentBrush, $"{solutionStep} / {solution.Count}");
        }

        private void OnKeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Space)
            {
                Step();
                args.Handled = true;

                return;
            }

            var direction = args.Key switch
            {
                Key.Up    => Direction.Up
              , Key.Right => Direction.Right
              , Key.Down  => Direction.Down
              , Key.Left  => Direction.Left
              , _         => (Direction?)null
            };

            if (direction is null) return;

            if (game.Move(direction.Value))
            {
                Render();
                Message("Moved by hand", Skin.MutedBrush, $"{game.Steps} steps · {game.Pushes} pushes");
            }
            else
            {
                Message("Blocked", Skin.DangerBrush);
            }

            args.Handled = true;
        }

        /// <summary> A ripple that expands from a square and fades , the only authored motion of the window </summary>
        private sealed class Pulse
        {
            public int      X;
            public int      Y;
            public double   Elapsed;
            public double   Delay;
            public double   Life  = 420;
            public IBrush   Brush = Skin.SuccessBrush;
            public Border   Ring;
        }

        private void Ripple(int x, int y, double delay = 0, IBrush brush = null)
        {
            pulses.Add(new Pulse { X = x, Y = y, Delay = delay, Brush = brush ?? Skin.SuccessBrush });

            if (!pulseTimer.IsEnabled) pulseTimer.Start();
        }

        private void PulseTick()
        {
            if (pulses.Count == 0)
            {
                pulseTimer.Stop();

                return;
            }

            var alive = new List<Pulse>();

            foreach (var pulse in pulses)
            {
                pulse.Elapsed += 16;

                if (pulse.Elapsed >= pulse.Life + pulse.Delay) continue;

                alive.Add(pulse);
            }

            var same = alive.Count == pulses.Count;

            pulses.Clear();
            pulses.AddRange(alive);

            // the ring is a live control now : move it every tick , repaint only when one is done
            foreach (var pulse in pulses) PaintPulse(pulse);

            if (!same) Render();
        }

        private static Border RippleVisual(Pulse pulse)
        {
            var ring = new Border
            {
                Width                 = Tile - 6,
                Height                = Tile - 6,
                CornerRadius          = new CornerRadius(13),
                BorderThickness       = new Thickness(2.5),
                BorderBrush           = pulse.Brush,
                RenderTransform       = new ScaleTransform(1, 1),
                RenderTransformOrigin = RelativePoint.Center,
                IsHitTestVisible      = false
            };

            Canvas.SetLeft(ring, Gap + pulse.X * Tile + 3);
            Canvas.SetTop(ring, Gap + pulse.Y * Tile + 3);

            pulse.Ring = ring;
            PaintPulse(pulse);

            return ring;
        }

        private static void PaintPulse(Pulse pulse)
        {
            if (pulse.Ring is null) return;

            var progress = Math.Clamp((pulse.Elapsed - pulse.Delay) / pulse.Life, 0, 1);

            if (pulse.Ring.RenderTransform is ScaleTransform scale)
            {
                var value = 0.9 + 1.15 * progress;

                scale.ScaleX = value;
                scale.ScaleY = value;
            }

            pulse.Ring.Opacity = 0.85 * (1 - progress);
        }

        private List<(int X, int Y)> HomeCells()
        {
            var cells = new List<(int X, int Y)>();

            foreach (var box in game.Boxes)
            {
                if (game.IsGoal(box.X, box.Y)) cells.Add((box.X, box.Y));
            }

            return cells;
        }

        private void Message(string text, IBrush color, string sub = null)
        {
            status.Text      = text;
            status.Foreground = color;
            detail.Text      = sub;
        }

        private void Render()
        {
            board.Children.Clear();

            stepsText.Text  = game.Steps.ToString();
            pushesText.Text = game.Pushes.ToString();
            goalsText.Text  = $"{Home()} / {game.Boxes.Count}";

            for (var y = 0 ; y < game.Height ; y++)
            {
                for (var x = 0 ; x < game.Width ; x++)
                {
                    var cell = Cell(x, y);

                    Canvas.SetLeft(cell, Gap + x * Tile);
                    Canvas.SetTop(cell, Gap + y * Tile);
                    board.Children.Add(cell);
                }
            }

            foreach (var pulse in pulses) board.Children.Add(RippleVisual(pulse));

            goalsText.Foreground = game.Solved ? Skin.SuccessBrush : Skin.InkBrush;
        }

        private Control Cell(int x, int y)
        {
            var inside = new Border
            {
                Width        = Tile - 4,
                Height       = Tile - 4,
                CornerRadius = new CornerRadius(9)
            };

            if (game.IsWall(x, y))
            {
                inside.Background = Skin.WallBrush;
                inside.Child      = new Border
                {
                    Height          = 4,
                    Margin          = new Thickness(5, 4, 5, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    CornerRadius    = new CornerRadius(2),
                    Background      = Skin.WallTopBrush
                };

                return inside;
            }

            inside.Background      = Skin.FloorBrush;
            inside.BorderBrush     = new SolidColorBrush(Color.Parse("#1D2230"));
            inside.BorderThickness = new Thickness(1.0);

            if (game.IsGoal(x, y))
            {
                inside.Child = new Border
                {
                    Width             = 18,
                    Height            = 18,
                    CornerRadius      = new CornerRadius(9),
                    BorderThickness   = new Thickness(2),
                    BorderBrush       = Skin.GoalRingBrush,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Center
                };
            }

            if (game.HasBox(x, y))
            {
                var onGoal = game.IsGoal(x, y);

                inside.Child = new Border
                {
                    Width             = 32,
                    Height            = 32,
                    CornerRadius      = new CornerRadius(8),
                    Background        = onGoal ? Skin.BoxDoneBrush : Skin.BoxBrush,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Center,
                    Child             = new Border
                    {
                        Height          = 6,
                        Margin          = new Thickness(5, 4, 5, 0),
                        VerticalAlignment = VerticalAlignment.Top,
                        CornerRadius    = new CornerRadius(3),
                        Background      = onGoal ? Skin.BoxDoneTopBrush : Skin.BoxTopBrush
                    }
                };
            }
            else if (game.PlayerX == x && game.PlayerY == y)
            {
                inside.Child = new Border
                {
                    Width             = 28,
                    Height            = 28,
                    CornerRadius      = new CornerRadius(14),
                    Background        = Skin.PlayerBrush,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Center,
                    Child             = new Border
                    {
                        Width             = 8,
                        Height            = 8,
                        CornerRadius      = new CornerRadius(4),
                        Margin            = new Thickness(6, 5, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment   = VerticalAlignment.Top,
                        Background        = Skin.PlayerGlint
                    }
                };
            }

            return inside;
        }
    }
}
