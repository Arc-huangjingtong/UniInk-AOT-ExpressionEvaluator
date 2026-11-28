using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Arc.UniInk.Sample.Sokoban.Core;

namespace Arc.UniInk.Sample.Sokoban.App
{
    /// <summary>
    /// A headless player : it prints the board , evaluates one UniInk policy per turn and shows what came back .
    /// Used to record how an agent plays — the window is not involved .
    /// </summary>
    public static class ConsolePlayer
    {
        private static readonly string[] Arrow = { "up", "right", "down", "left" };

        private static bool Color => !Console.IsOutputRedirected;

        private static string Paint(string text, string code) => Color ? $"\u001b[{code}m{text}\u001b[0m" : text;

        private static string Dim(string text) => Paint(text, "90");

        private static string Cyan(string text) => Paint(text, "36");

        private static string Green(string text) => Paint(text, "32");

        private static string Red(string text) => Paint(text, "31");

        private static string Bold(string text) => Paint(text, "1");

        public static bool TryRun(string[] args)
        {
            switch (args.Length > 0 ? args[0] : string.Empty)
            {
                case "--list" :
                    List();

                    return true;
                case "--solve" :
                    Solve(args.Length > 1 ? int.Parse(args[1]) : Levels.Count - 1);

                    return true;
                case "--play" :
                    Play(args.Length > 1 ? int.Parse(args[1]) : Levels.Count - 1, args.Skip(2));

                    return true;
                case "--ask" :
                    Ask(args.Length > 1 ? int.Parse(args[1]) : Levels.Count - 1, args.Skip(2));

                    return true;
                case "--script" :
                    Play(args.Length > 1 ? int.Parse(args[1]) : Levels.Count - 1,
                         File.ReadAllLines(args.Length > 2 ? args[2] : "policies.txt")
                             .Where(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("#")),
                         args.Length > 3 ? int.Parse(args[3]) : 0);

                    return true;
                default :
                    return false;
            }
        }

        private static void List()
        {
            for (var i = 0 ; i < Levels.Count ; i++)
            {
                var level = Levels.Get(i);

                Console.WriteLine($"  {i}  {level.Name,-15} {level.Width} x {level.Height}  {level.Hint}");
            }
        }

        /// <summary> Ask the board questions : each expression is evaluated , nothing moves </summary>
        private static void Ask(int index, IEnumerable<string> expressions)
        {
            var game = new Game(Levels.Get(index));

            Console.WriteLine(Bold($"level {index} : {game.Level.Name} ( {game.Width} x {game.Height} )"));
            Console.WriteLine(game.Render());

            foreach (var expression in expressions)
            {
                var ink   = new Protocol(game).CreateInk();
                var start = string.Empty;

                try
                {
                    var value = ink.Evaluate_IfStatement(expression) as Arc.UniInk.InkValue;

                    Console.WriteLine($"  ask  {expression}");
                    Console.WriteLine($"  back {Describe(value)}");
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"  ask  {expression}");
                    Console.WriteLine($"  back {exception.GetType().Name}: {exception.Message}");
                }
            }
        }

        private static string Describe(Arc.UniInk.InkValue value)
        {
            if (value is null) return "null";

            return value.ValueType switch
            {
                TypeCode.Int32   => value.GetResult_Int().ToString()
              , TypeCode.Boolean => value.GetResult_Bool().ToString()
              , TypeCode.Single  => value.GetResult_Float().ToString("0.###") + "f"
              , TypeCode.Double  => value.GetResult_Double().ToString("0.###")
              , _                => value.GetResult_String() ?? value.ValueType.ToString()
            };
        }

        private static void Solve(int index)
        {
            var game = new Game(Levels.Get(index));
            var path = Solver.Solve(game);

            if (path is null)
            {
                Console.WriteLine("no solution");

                return;
            }

            Console.WriteLine($"{game.Level.Name} : {path.Count} moves");
            Console.WriteLine(string.Concat(path.Select(direction => "URDL"[(int)direction])));
        }

        /// <summary> One evaluation per policy line , until the level is solved or a turn fails </summary>
        private static void Play(int index, IEnumerable<string> policies, int delay = 0)
        {
            var game     = new Game(Levels.Get(index));
            var turn     = 0;
            var policies_ = policies.ToList();

            Console.WriteLine(Bold($"level {index} : {game.Level.Name} ( {game.Width} x {game.Height} )"));
            Console.WriteLine(game.Render());

            foreach (var policy in policies_)
            {
                if (game.Solved)
                {
                    Console.WriteLine("the level is already solved");

                    break;
                }

                turn++;

                if (delay > 0) System.Threading.Thread.Sleep(delay);

                Console.WriteLine(Bold($"── turn {turn} ─────────────────────────────────────────────"));
                Console.WriteLine($"{Dim("expr")} : {Cyan(policy)}");

                var scheme = policy.Contains("W") || policy.Contains("A") || policy.Contains("S") ? InputScheme.Wasd : InputScheme.UrDL;
                var result = new Protocol(game, scheme).Evaluate(policy);

                if (!result.Ok)
                {
                    Console.WriteLine(Red($"fail : {result.Error}"));

                    break;
                }

                Console.WriteLine($"{Dim("moves")} : {string.Join(" ", result.Moves.Select(direction => Arrow[(int)direction]))}" +
                                  Dim(result.FromAction ? "  ( action letters )" : "  ( direction code )"));

                var played = 0;

                foreach (var direction in result.Moves)
                {
                    if (!game.Move(direction))
                    {
                        Console.WriteLine(Red($"stop : {Arrow[(int)direction]} is blocked after {played} move(s)"));
                        Print(game);

                        return;
                    }

                    played++;
                }

                Print(game);

                if (game.Solved)
                {
                    Console.WriteLine(Green($"solved in {game.Steps} steps with {game.Pushes} pushes"));

                    return;
                }
            }

            Console.WriteLine("the script ended before the level was solved");
            Console.WriteLine($"steps {game.Steps} · pushes {game.Pushes} · boxes home {Home(game)} of {game.Boxes.Count}");
        }

        private static void Print(Game game)
        {
            Console.WriteLine(game.Render());
            Console.WriteLine(Dim($"[ px {game.PlayerX} py {game.PlayerY} · steps {game.Steps} · pushes {game.Pushes} · boxes {Home(game)}/{game.Boxes.Count} ]"));
        }

        private static int Home(Game game)
        {
            var home = 0;

            foreach (var box in game.Boxes)
            {
                if (game.IsGoal(box.X, box.Y)) home++;
            }

            return home;
        }
    }
}
