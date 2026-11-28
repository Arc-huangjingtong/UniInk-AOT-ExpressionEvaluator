using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Threading;
using Arc.UniInk.Sample.Sokoban.Core;

namespace Arc.UniInk.Sample.Sokoban.App
{
    /// <summary>
    /// Plays a policy script as if an agent were working in the window : it types a line , writes it out one move at
    /// a time with a thinking beat , deletes the line quickly and types the next one .
    /// </summary>
    public sealed class Replayer
    {
        private enum Phase
        {
            Type,
            Think,
            Move,
            Delete,
            Done
        }

        private const int TickMs = 40;

        private readonly MainWindow window;
        private readonly IReadOnlyList<string> policies;
        private readonly DispatcherTimer timer;

        private int index;
        private int chars;
        private int beat;
        private Phase phase = Phase.Type;

        public Replayer(MainWindow window, IReadOnlyList<string> policies)
        {
            this.window   = window;
            this.policies = policies;
            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(TickMs) };
            timer.Tick += (_, _) => Tick();
        }

        public void Start()
        {
            if (policies.Count == 0) return;

            window.DemoSetPolicy(string.Empty);
            timer.Start();
        }

        private void Tick()
        {
            switch (phase)
            {
                case Phase.Type :
                    chars += 2;

                    if (chars <= Current.Length)
                    {
                        window.DemoSetPolicy(Current.Substring(0, chars) + "▌");
                    }
                    else
                    {
                        window.DemoSetPolicy(Current);
                        window.DemoBeginTurn();
                        phase = Phase.Think;
                        beat  = 0;
                    }

                    return;

                case Phase.Think :
                    window.DemoThink(beat);

                    if (++beat > 2)
                    {
                        phase = Phase.Move;
                        beat  = 0;
                    }

                    return;

                case Phase.Move :
                    window.DemoPlayOne();

                    if (window.DemoMovesLeft() > 0)
                    {
                        phase = Phase.Think;
                        beat  = 0;
                    }
                    else
                    {
                        phase = Phase.Delete;
                        chars = Current.Length;
                    }

                    return;

                case Phase.Delete :
                    // deleting is faster than typing : 4 characters per tick
                    chars -= 4;
                    window.DemoSetPolicy(chars > 0 ? Current.Substring(0, chars) : string.Empty);

                    if (chars <= 0) Next();

                    return;
            }
        }

        private void Next()
        {
            index++;

            if (index >= policies.Count)
            {
                window.DemoCelebrate();
                phase = Phase.Done;
                timer.Stop();

                return;
            }

            chars = 0;
            phase = Phase.Type;
        }

        private string Current => policies[index < 0 ? 0 : index];
    }
}
