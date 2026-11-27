using System;
using System.Collections.Generic;
using System.Text;

namespace Arc.UniInk.Sample.Sokoban.Core
{
    /// <summary> One playable level : the player , the boxes and the rules </summary>
    public sealed class Game
    {
        private readonly Tile[,] tiles;
        private readonly List<(int X, int Y)> boxes;

        public Game(Level level)
        {
            Level = level;
            tiles = level.Tiles;
            boxes = new List<(int X, int Y)>(level.InitialBoxes);

            PlayerX = level.PlayerX;
            PlayerY = level.PlayerY;
        }

        public Level Level { get; }

        public int PlayerX { get; private set; }

        public int PlayerY { get; private set; }

        public int Width => Level.Width;

        public int Height => Level.Height;

        public int Steps { get; private set; }

        public int Pushes { get; private set; }

        public IReadOnlyList<(int X, int Y)> Boxes => boxes;

        public bool Solved
        {
            get
            {
                foreach (var box in boxes)
                {
                    if (tiles[box.X, box.Y] != Tile.Goal) return false;
                }

                return boxes.Count > 0;
            }
        }

        public bool IsWall(int x, int y) => tiles[x, y] == Tile.Wall;

        public bool IsGoal(int x, int y) => tiles[x, y] == Tile.Goal;

        public bool HasBox(int x, int y)
        {
            foreach (var box in boxes)
            {
                if (box.X == x && box.Y == y) return true;
            }

            return false;
        }

        /// <summary> The square the player would step on , null when it is outside the level </summary>
        public static (int X, int Y) Step(int x, int y, Direction direction) => direction switch
        {
            Direction.Up    => (x, y - 1)
          , Direction.Right => (x + 1, y)
          , Direction.Down  => (x, y + 1)
          , _               => (x - 1, y)
        };

        public bool CanMove(Direction direction)
        {
            var (nx, ny) = Step(PlayerX, PlayerY, direction);

            if (IsWall(nx, ny)) return false;
            if (!HasBox(nx, ny)) return true;

            var (bx, by) = Step(nx, ny, direction);

            return !IsWall(bx, by) && !HasBox(bx, by);
        }

        /// <summary> Apply a move , returns false when it is not legal </summary>
        public bool Move(Direction direction)
        {
            if (!CanMove(direction)) return false;

            var (nx, ny) = Step(PlayerX, PlayerY, direction);

            if (HasBox(nx, ny))
            {
                var (bx, by) = Step(nx, ny, direction);
                var index    = boxes.FindIndex(box => box.X == nx && box.Y == ny);

                boxes[index] = (bx, by);
                Pushes++;
            }

            PlayerX = nx;
            PlayerY = ny;
            Steps++;

            return true;
        }

        /// <summary> The board as text : `#` wall , `.` goal , `$` box , `*` box on goal , `@` player </summary>
        public string Render()
        {
            var builder = new StringBuilder((Width + 1) * Height);

            for (var y = 0 ; y < Height ; y++)
            {
                for (var x = 0 ; x < Width ; x++)
                {
                    var tile = tiles[x, y];
                    var box  = HasBox(x, y);
                    var me   = PlayerX == x && PlayerY == y;

                    builder.Append(tile switch
                    {
                        Tile.Wall => '#'
                      , Tile.Goal => box ? '*' : me ? '+' : '.'
                      , _         => box ? '$' : me ? '@' : ' '
                    });
                }

                builder.Append('\n');
            }

            return builder.ToString();
        }
    }
}
