using System;
using System.Collections.Generic;

namespace UniInk.Sample.Sokoban.Core
{
    /// <summary> One square of the level </summary>
    public enum Tile
    {
        Floor,
        Wall,
        Goal
    }

    /// <summary> The four directions a policy may return </summary>
    public enum Direction
    {
        Up    = 0,
        Right = 1,
        Down  = 2,
        Left  = 3
    }

    /// <summary> Immutable level data : the walls and the goals of one level </summary>
    public sealed class Level
    {
        public Level(string name, string[] rows, int playerX, int playerY, string hint = "")
        {
            Name  = name;
            Hint  = hint;
            Width = rows[0].Length;
            Height = rows.Length;
            Tiles = new Tile[Width, Height];

            for (var y = 0 ; y < Height ; y++)
            {
                if (rows[y].Length != Width) throw new ArgumentException($"row {y} is not {Width} wide");

                for (var x = 0 ; x < Width ; x++)
                {
                    Tiles[x, y] = rows[y][x] switch
                    {
                        '#' => Tile.Wall
                      , '.' => Tile.Goal
                      , ' ' => Tile.Floor
                      , _   => throw new ArgumentException($"unknown level character '{rows[y][x]}'")
                    };
                }
            }

            PlayerX = playerX;
            PlayerY = playerY;
        }

        public string Name { get; }

        public string Hint { get; }

        public int Width { get; }

        public int Height { get; }

        public Tile[,] Tiles { get; }

        public int PlayerX { get; }

        public int PlayerY { get; }

        public Tile this[int x, int y] => x < 0 || y < 0 || x >= Width || y >= Height ? Tile.Wall : Tiles[x, y];

        /// <summary> Parse the compact text form used by the level list </summary>
        /// <remarks>
        /// `#` wall , `.` goal , ` ` floor , `@` the player , `$` a box
        /// </remarks>
        public static Level Parse(string name, string[] rows, string hint = "")
        {
            var boxes = new List<(int X, int Y)>();
            var playerX = -1;
            var playerY = -1;
            var clean   = new string[rows.Length];

            for (var y = 0 ; y < rows.Length ; y++)
            {
                var line = rows[y].ToCharArray();

                for (var x = 0 ; x < line.Length ; x++)
                {
                    switch (line[x])
                    {
                        case '@' :
                            playerX = x;
                            playerY = y;
                            line[x] = ' ';

                            break;
                        case '$' :
                            boxes.Add((x, y));
                            line[x] = ' ';

                            break;
                        case '+' : // a box already on a goal
                            boxes.Add((x, y));
                            line[x] = '.';

                            break;
                        case '*' : // the player on a goal
                            playerX = x;
                            playerY = y;
                            line[x] = '.';

                            break;
                    }
                }

                clean[y] = new string(line);
            }

            if (playerX < 0) throw new ArgumentException("the level has no player");

            var level = new Level(name, clean, playerX, playerY, hint);
            level.InitialBoxes = boxes.ToArray();

            return level;
        }

        /// <summary> The boxes in the text form , set by <see cref="Parse"/> </summary>
        public (int X, int Y)[] InitialBoxes { get; private set; } = Array.Empty<(int, int)>();
    }
}
