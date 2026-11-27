using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arc.UniInk.Sample.Sokoban.Core
{
    /// <summary> Breadth first solver , used to prove that a level is solvable and to drive the "Solve" button </summary>
    public static class Solver
    {
        private static readonly Direction[] Directions =
        {
            Direction.Up, Direction.Right, Direction.Down, Direction.Left
        };

        /// <summary> The shortest move sequence , or null when the level cannot be solved within the budget </summary>
        public static List<Direction> Solve(Game game, int maxStates = 200_000)
        {
            var start = Snapshot(game);

            if (IsSolved(game, start)) return new List<Direction>();

            var queue   = new Queue<State>();
            var seen    = new HashSet<string> { Key(game, start) };
            var parents = new Dictionary<string, (string Parent, Direction Move)>();

            queue.Enqueue(start);

            var expanded = 0;

            while (queue.Count > 0 && expanded++ < maxStates)
            {
                var current = queue.Dequeue();
                var currentKey = Key(game, current);

                foreach (var direction in Directions)
                {
                    var next = Move(game, current, direction);

                    if (next is null) continue;

                    var nextKey = Key(game, next);

                    if (!seen.Add(nextKey)) continue;

                    parents[nextKey] = (currentKey, direction);

                    if (IsSolved(game, next)) return Rebuild(game, parents, nextKey);

                    queue.Enqueue(next);
                }
            }

            return null;
        }

        private static List<Direction> Rebuild(Game game, Dictionary<string, (string Parent, Direction Move)> parents, string key)
        {
            var path = new List<Direction>();

            while (parents.TryGetValue(key, out var link))
            {
                path.Add(link.Move);
                key = link.Parent;
            }

            path.Reverse();

            return path;
        }

        /// <summary> Apply a move to a snapshot , null when it is illegal </summary>
        private static State Move(Game game, State state, Direction direction)
        {
            var (nx, ny) = Game.Step(state.X, state.Y, direction);

            if (game.IsWall(nx, ny)) return null;

            var boxes = (int[])state.Boxes.Clone();

            for (var i = 0 ; i < boxes.Length ; i += 2)
            {
                if (boxes[i] != nx || boxes[i + 1] != ny) continue;

                var (bx, by) = Game.Step(nx, ny, direction);

                if (game.IsWall(bx, by) || HasBox(boxes, bx, by)) return null;

                boxes[i]     = bx;
                boxes[i + 1] = by;

                return new State(nx, ny, boxes);
            }

            return new State(nx, ny, boxes);
        }

        private static bool IsSolved(Game game, State state)
        {
            for (var i = 0 ; i < state.Boxes.Length ; i += 2)
            {
                if (game.Level[state.Boxes[i], state.Boxes[i + 1]] != Tile.Goal) return false;
            }

            return state.Boxes.Length > 0;
        }

        private static State Snapshot(Game game)
        {
            var boxes = new int[game.Boxes.Count * 2];

            for (var i = 0 ; i < game.Boxes.Count ; i++)
            {
                boxes[i * 2]     = game.Boxes[i].X;
                boxes[i * 2 + 1] = game.Boxes[i].Y;
            }

            return new State(game.PlayerX, game.PlayerY, boxes);
        }

        /// <summary> The boxes are sorted in the key , so two states with the same boxes match </summary>
        private static string Key(Game game, State state)
        {
            var pairs = new int[state.Boxes.Length / 2];

            for (var i = 0 ; i < pairs.Length ; i++) pairs[i] = state.Boxes[i * 2] * 1000 + state.Boxes[i * 2 + 1];

            Array.Sort(pairs);

            var builder = new StringBuilder();

            builder.Append(state.X).Append(',').Append(state.Y).Append('|');

            foreach (var pair in pairs) builder.Append(pair).Append(';');

            return builder.ToString();
        }

        private static bool HasBox(int[] boxes, int x, int y)
        {
            for (var i = 0 ; i < boxes.Length ; i += 2)
            {
                if (boxes[i] == x && boxes[i + 1] == y) return true;
            }

            return false;
        }

        private sealed class State
        {
            public readonly int X;
            public readonly int Y;
            public readonly int[] Boxes;

            public State(int x, int y, int[] boxes)
            {
                X = x;
                Y = y;
                Boxes = boxes;
            }
        }
    }
}
