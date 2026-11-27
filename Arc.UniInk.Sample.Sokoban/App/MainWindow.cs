using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Arc.UniInk.Sample.Sokoban.Core;

namespace Arc.UniInk.Sample.Sokoban.App
{
    /// <summary> The window : the board , a policy box and one evaluation per turn </summary>
    public sealed class MainWindow : Window
    {
        private const double CellSize = 42;

        private static readonly IBrush WallBrush     = new SolidColorBrush(Color.Parse("#3C3C3C"));
        private static readonly IBrush FloorBrush    = new SolidColorBrush(Color.Parse("#F6F1E7"));
        private static readonly IBrush GoalBrush     = new SolidColorBrush(Color.Parse("#CDE9C9"));
        private static readonly IBrush BoxBrush      = new SolidColorBrush(Color.Parse("#A1887F"));
        private static readonly IBrush BoxDoneBrush  = new SolidColorBrush(Color.Parse("#4CAF50"));
        private static readonly IBrush PlayerBrush   = new SolidColorBrush(Color.Parse("#1E88E5"));
        private static readonly IBrush TextBrush     = Brushes.White;

        private readonly ComboBox levelBox  = new() { Width = 220, Margin = new Thickness(0, 0, 8, 0) };
        private readonly TextBox  policyBox = new() { AcceptsReturn = true, MinHeight = 90, FontFamily = FontFamily.Default };
        private readonly TextBlock status   = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0) };
        private readonly CheckBox  wasd     = new() { Content = "WASD" };
        private readonly Canvas board      = new() { Width = 1, Height = 1 };
        private readonly DispatcherTimer timer;
        private readonly DispatcherTimer solutionTimer;

        private Game game;
        private List<Direction> solution;
        private int solutionStep;

        public MainWindow()
        {
            Title = "UniInk Sokoban — one expression per turn";
            Width = 980;
            Height = 640;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(320) };
            timer.Tick += (_, _) => Step();

            solutionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(220) };
            solutionTimer.Tick += (_, _) => PlaySolutionStep();

            Content = BuildLayout();

            for (var i = 0 ; i < Levels.Count ; i++) levelBox.Items.Add(Levels.Get(i).Name);

            levelBox.SelectedIndex = 0;
            levelBox.SelectionChanged += (_, _) => LoadLevel(levelBox.SelectedIndex);

            policyBox.Text = "if ( BOX_AT( right ) && ! WALL( px + 2 , py ) ) { return right } else { return down }";

            LoadLevel(0);

            KeyDown += OnKeyDown;
        }

        private Control BuildLayout()
        {
            var runTurn  = new Button { Content = "Run one turn" };
            var auto     = new CheckBox { Content = "Auto run" };
            var solve    = new Button { Content = "Solve ( BFS )" };
            var reset    = new Button { Content = "Reset" };

            runTurn.Click += (_, _) => Step();
            auto.IsCheckedChanged += (_, _) =>
            {
                if (auto.IsChecked == true) timer.Start();
                else                        timer.Stop();
            };
            solve.Click += (_, _) => SolveLevel();
            reset.Click += (_, _) => LoadLevel(levelBox.SelectedIndex);

            var toolbar = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing     = 8,
                Margin      = new Thickness(12, 12, 12, 0),
                Children    = { levelBox, runTurn, auto, solve, wasd, reset }
            };

            var boardHost = new Border
            {
                Background      = new SolidColorBrush(Color.Parse("#2B2B2B")),
                CornerRadius    = new CornerRadius(8),
                Padding         = new Thickness(14),
                Margin          = new Thickness(12),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment   = VerticalAlignment.Top,
                Child           = board
            };

            var hint = new TextBlock
            {
                Text         = "Arrows also move the player. The policy is one UniInk expression, evaluated once per turn; " +
                               "it must return a direction: up / right / down / left ( 0 / 1 / 2 / 3 ).",
                TextWrapping = TextWrapping.Wrap,
                Opacity      = 0.75,
                Margin       = new Thickness(0, 6, 0, 0)
            };

            var side = new StackPanel
            {
                Margin   = new Thickness(0, 12, 12, 12),
                Spacing  = 6,
                Children = { new TextBlock { Text = "Policy" }, policyBox, hint, status }
            };

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,380"),
                RowDefinitions    = new RowDefinitions("Auto,*")
            };

            Grid.SetColumn(toolbar, 0);
            Grid.SetColumnSpan(toolbar, 2);
            Grid.SetRow(toolbar, 0);

            Grid.SetColumn(boardHost, 0);
            Grid.SetRow(boardHost, 1);

            Grid.SetColumn(side, 1);
            Grid.SetRow(side, 1);

            grid.Children.Add(toolbar);
            grid.Children.Add(boardHost);
            grid.Children.Add(side);

            return grid;
        }

        private void LoadLevel(int index)
        {
            timer.Stop();
            solutionTimer.Stop();
            solution = null;

            game = new Game(Levels.Get(index < 0 ? 0 : index));

            board.Width  = game.Width * CellSize;
            board.Height = game.Height * CellSize;

            policyBox.Text = policyBox.Text;   // keep the policy across levels
            Render();
            Status(game.Level.Hint, false);
        }

        /// <summary> Evaluate the policy once and apply the move it answers with </summary>
        private void Step()
        {
            if (game.Solved)
            {
                timer.Stop();

                Status("Solved — reset or pick another level.", true);

                return;
            }

            var scheme = wasd.IsChecked == true ? InputScheme.Wasd : InputScheme.UrDL;
            var turn   = new Protocol(game, scheme).Evaluate(policyBox.Text);

            if (!turn.Ok)
            {
                timer.Stop();
                Status($"policy error: {turn.Error}", true);

                return;
            }

            var moved = 0;

            foreach (var direction in turn.Moves)
            {
                if (!game.Move(direction)) break;

                moved++;
            }

            Render();

            if (moved < turn.Moves.Count)
            {
                timer.Stop();
                Status($"moved {moved}/{turn.Moves.Count} , then {turn.Moves[moved]} is blocked", true);

                return;
            }

            var how = turn.FromAction ? $" ({turn.Moves.Count} action getter{ (turn.Moves.Count == 1 ? "" : "s" ) })" : string.Empty;

            Status(game.Solved ? "Solved!" : $"{turn.Moves.Count} move{ (turn.Moves.Count == 1 ? "" : "s" ) }{how} · steps {game.Steps} · pushes {game.Pushes}", game.Solved);
        }

        /// <summary> Play the shortest solution from the solver , one move per tick </summary>
        private void SolveLevel()
        {
            if (game.Solved) return;

            timer.Stop();
            solution     = Solver.Solve(game);
            solutionStep = 0;

            if (solution is null || solution.Count == 0)
            {
                Status("no solution found", true);

                return;
            }

            solutionTimer.Start();
        }

        private void PlaySolutionStep()
        {
            if (solution is null || solutionStep >= solution.Count || game.Solved)
            {
                solutionTimer.Stop();
                Status(game.Solved ? "Solved by BFS!" : "the solution ran out", game.Solved);

                return;
            }

            game.Move(solution[solutionStep++]);
            Render();
            Status($"solver {solutionStep}/{solution.Count} · steps {game.Steps} · pushes {game.Pushes}", game.Solved);
        }

        private void OnKeyDown(object sender, KeyEventArgs args)
        {
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
                Status($"{direction} · steps {game.Steps} · pushes {game.Pushes}", game.Solved);
            }
            else
            {
                Status("blocked", false);
            }

            args.Handled = true;
        }

        private void Status(string text, bool error) => status.Text = text;

        private void Render()
        {
            board.Children.Clear();

            for (var y = 0 ; y < game.Height ; y++)
            {
                for (var x = 0 ; x < game.Width ; x++)
                {
                    var background = game.IsWall(x, y)
                                         ? WallBrush
                                         : game.IsGoal(x, y)
                                             ? GoalBrush
                                             : FloorBrush;

                    var cell = new Border
                    {
                        Width           = CellSize,
                        Height          = CellSize,
                        Background      = background,
                        BorderThickness = new Thickness(0.5),
                        BorderBrush     = new SolidColorBrush(Color.Parse("#DDD6C8")),
                        Child           = Glyph(x, y)
                    };

                    Canvas.SetLeft(cell, x * CellSize);
                    Canvas.SetTop(cell, y * CellSize);
                    board.Children.Add(cell);
                }
            }
        }

        private Control Glyph(int x, int y)
        {
            if (game.IsWall(x, y)) return null;

            if (game.HasBox(x, y))
            {
                return new Border
                {
                    Margin          = new Thickness(6),
                    CornerRadius    = new CornerRadius(4),
                    Background      = game.IsGoal(x, y) ? BoxDoneBrush : BoxBrush
                };
            }

            if (game.PlayerX == x && game.PlayerY == y)
            {
                return new Border
                {
                    Margin       = new Thickness(9),
                    CornerRadius = new CornerRadius(20),
                    Background   = PlayerBrush
                };
            }

            return null;
        }
    }
}
