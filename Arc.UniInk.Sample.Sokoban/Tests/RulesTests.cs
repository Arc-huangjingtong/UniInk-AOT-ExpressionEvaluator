using NUnit.Framework;
using Arc.UniInk.Sample.Sokoban.Core;

namespace Arc.UniInk.Sample.Sokoban.Tests
{
    public sealed class RulesTests
    {
        [Test]
        public void Player_CannotWalkIntoAWall()
        {
            var game = new Game(Levels.Get(0));   // ##### / #@$. # / #####

            Assert.That(game.PlayerX, Is.EqualTo(1));
            Assert.That(game.CanMove(Direction.Left), Is.False, "the left neighbour is a wall");
            Assert.That(game.CanMove(Direction.Up), Is.False, "the upper neighbour is a wall");
        }

        [Test]
        public void PushingABoxCountsAPushAndAStep()
        {
            var game = new Game(Levels.Get(0));

            Assert.That(game.Move(Direction.Right), Is.True);
            Assert.That(game.Pushes, Is.EqualTo(1));
            Assert.That(game.Steps, Is.EqualTo(1));
            Assert.That(game.HasBox(3, 1), Is.True);
            Assert.That(game.Solved, Is.True);
        }

        [Test]
        public void ABoXCannotBePushedIntoAWall()
        {
            var game = new Game(Levels.Get(0));

            game.Move(Direction.Right);   // the box is on the goal , the square behind it is a wall

            Assert.That(game.CanMove(Direction.Right), Is.False);
            Assert.That(game.Move(Direction.Right), Is.False);
        }

        [Test]
        public void SolvedNeedsEveryBoxOnAGoal()
        {
            var game = new Game(Levels.Get(2));   // two boxes , two goals

            Assert.That(game.Solved, Is.False);
        }

        [Test]
        public void RenderShowsTheBoxOnTheGoal()
        {
            var game = new Game(Levels.Get(0));

            game.Move(Direction.Right);

            Assert.That(game.Render(), Does.Contain("*"));
        }

        [Test]
        public void EveryLevelIsSolvableShape()
        {
            foreach (var level in Levels.All())
            {
                Assert.That(level.InitialBoxes.Length, Is.GreaterThan(0), $"{level.Name} has no box");
                Assert.That(level.PlayerX, Is.GreaterThanOrEqualTo(0), $"{level.Name} has no player");
            }
        }
    }
}
