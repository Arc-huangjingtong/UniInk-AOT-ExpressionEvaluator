using NUnit.Framework;
using Arc.UniInk.Sample.Sokoban.Core;

namespace Arc.UniInk.Sample.Sokoban.Tests
{
    public sealed class SolverTests
    {
        [Test]
        public void EveryLevelIsSolvable()
        {
            for (var i = 0 ; i < Levels.Count ; i++)
            {
                var game  = new Game(Levels.Get(i));
                var path  = Solver.Solve(game, 400_000);

                Assert.That(path, Is.Not.Null, $"level {i} ({game.Level.Name}) has no solution");

                foreach (var direction in path)
                {
                    Assert.That(game.Move(direction), Is.True, $"level {i} replays an illegal move");
                }

                Assert.That(game.Solved, Is.True, $"level {i} does not end solved");
            }
        }

        [Test]
        public void TheFirstLevelNeedsOneMove()
        {
            var path = Solver.Solve(new Game(Levels.Get(0)));

            Assert.That(path, Has.Count.EqualTo(1));
            Assert.That(path[0], Is.EqualTo(Direction.Right));
        }
    }
}
