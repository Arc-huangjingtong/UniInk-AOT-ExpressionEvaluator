using NUnit.Framework;
using Arc.UniInk.Sample.Sokoban.Core;

namespace Arc.UniInk.Sample.Sokoban.Tests
{
    public sealed class ProtocolTests
    {
        private static Game NewGame(int level = 0) => new(Levels.Get(level));

        [Test]
        public void VariablesExposeTheBoard()
        {
            var game = NewGame();
            var ink  = new Protocol(game).CreateInk();

            Assert.That(ink.Evaluate("px").GetResult_Int(), Is.EqualTo(1));
            Assert.That(ink.Evaluate("py").GetResult_Int(), Is.EqualTo(1));
            Assert.That(ink.Evaluate("solved").GetResult_Int(), Is.EqualTo(0));
            Assert.That(ink.Evaluate("right").GetResult_Int(), Is.EqualTo((int)Direction.Right));
        }

        [Test]
        public void FunctionsAnswerAboutTheGrid()
        {
            var game = NewGame();
            var ink  = new Protocol(game).CreateInk();

            Assert.That(ink.Evaluate("WALL( 0 , 1 )").GetResult_Bool(), Is.True);
            Assert.That(ink.Evaluate("BOX( 2 , 1 )").GetResult_Bool(), Is.True);
            Assert.That(ink.Evaluate("GOAL( 3 , 1 )").GetResult_Bool(), Is.True);
            Assert.That(ink.Evaluate("FREE( 2 , 1 )").GetResult_Bool(), Is.False, "the box occupies it");
            Assert.That(ink.Evaluate("CAN_MOVE( right )").GetResult_Bool(), Is.True);
            Assert.That(ink.Evaluate("BOX_AT( right )").GetResult_Bool(), Is.True);
        }

        [Test]
        public void APolicyPushesTheBoxHome()
        {
            var game = NewGame();
            var turn = new Protocol(game).Evaluate("if ( CAN_MOVE( right ) ) { return right } else { return left }");

            Assert.That(turn.Ok, Is.True);
            Assert.That(turn.Direction, Is.EqualTo(Direction.Right));

            game.Move(turn.Direction);

            Assert.That(game.Solved, Is.True);
        }

        [Test]
        public void TheActionGetterMovesRight()
        {
            var game     = NewGame();
            var protocol = new Protocol(game);
            var turn     = protocol.Evaluate("R");

            Assert.That(turn.Ok, Is.True, turn.Error);
            Assert.That(turn.FromAction, Is.True);
            Assert.That(turn.Direction, Is.EqualTo(Direction.Right));
            Assert.That(protocol.Requested, Is.EqualTo(new[] { Direction.Right }));

            game.Move(turn.Direction);

            Assert.That(game.Solved, Is.True);
        }

        [Test]
        public void TheActionGettersReportLegality()
        {
            var protocol = new Protocol(NewGame());

            // the first level : a wall left and up , a box right , the player can only go right
            Assert.That(protocol.Evaluate("L").Direction, Is.EqualTo(Direction.Left));
            Assert.That(protocol.Evaluate("U").Direction, Is.EqualTo(Direction.Up));

            var ink = protocol.CreateInk();

            Assert.That(ink.Evaluate("R").GetResult_Int(), Is.EqualTo(1), "pushing right is legal");
            Assert.That(ink.Evaluate("L").GetResult_Int(), Is.EqualTo(0), "the left neighbour is a wall");
        }

        [Test]
        public void BothArmsQueueTheirAction()
        {
            // both arms are parsed before the condition is evaluated , so R and D both run and D wins
            var protocol = new Protocol(NewGame());
            var turn     = protocol.Evaluate("if ( BOX_AT( right ) ) { return R } else { return D }");

            Assert.That(turn.Ok, Is.True, turn.Error);
            Assert.That(turn.FromAction, Is.True);
            Assert.That(turn.Moves, Is.EqualTo(new[] { Direction.Right, Direction.Down }), "both arms queue their action");
        }

        [Test]
        public void RR_IsTwoMoves()
        {
            // the corridor level : #@$...# — two pushes to the right are legal
            var game     = NewGame(3);
            var protocol = new Protocol(game);
            var turn     = protocol.Evaluate("RR");

            Assert.That(turn.Ok, Is.True, turn.Error);
            Assert.That(turn.FromAction, Is.True);
            Assert.That(turn.Moves, Has.Count.EqualTo(2));
            Assert.That(turn.Moves[0], Is.EqualTo(Direction.Right));
            Assert.That(turn.Moves[1], Is.EqualTo(Direction.Right));

            foreach (var direction in turn.Moves) game.Move(direction);

            Assert.That(game.PlayerX, Is.EqualTo(3));
            Assert.That(game.Pushes, Is.EqualTo(2));
        }

        [Test]
        public void RL_IsRightThenLeft()
        {
            var game     = NewGame(3);
            var protocol = new Protocol(game);
            var turn     = protocol.Evaluate("RL");

            Assert.That(turn.Moves, Is.EqualTo(new[] { Direction.Right, Direction.Left }));

            foreach (var direction in turn.Moves) game.Move(direction);

            Assert.That(game.PlayerX, Is.EqualTo(1), "the pair cancels out");
        }

        [Test]
        public void ABlockedMoveStopsTheSequence()
        {
            // the first level : the player can push right once , then the box is against the wall
            var game     = NewGame();
            var protocol = new Protocol(game);
            var turn     = protocol.Evaluate("RRR");

            Assert.That(turn.Moves, Has.Count.EqualTo(3));

            var moved = 0;

            foreach (var direction in turn.Moves)
            {
                if (!game.Move(direction)) break;

                moved++;
            }

            Assert.That(moved, Is.EqualTo(1), "the first push works , the next two are blocked");
        }

        [Test]
        public void TheActionRequestResetsBetweenTurns()
        {
            var protocol = new Protocol(NewGame());

            Assert.That(protocol.Evaluate("R").FromAction, Is.True);
            Assert.That(protocol.Evaluate("1").FromAction, Is.False, "a plain direction code must not inherit the request");
            Assert.That(protocol.Requested, Is.Empty);
        }

        [Test]
        public void AnEmptyPolicyIsReported()
        {
            var turn = new Protocol(NewGame()).Evaluate("   ");

            Assert.That(turn.Ok, Is.False);
            Assert.That(turn.Error, Does.Contain("empty"));
        }

        [Test]
        public void AWrongResultTypeIsReported()
        {
            var turn = new Protocol(NewGame()).Evaluate("\"north\"");

            Assert.That(turn.Ok, Is.False);
            Assert.That(turn.Error, Does.Contain("the value type is not"));
        }

        [Test]
        public void AnOutOfRangeDirectionIsReported()
        {
            var turn = new Protocol(NewGame()).Evaluate("9");

            Assert.That(turn.Ok, Is.False);
            Assert.That(turn.Error, Does.Contain("expected a direction"));
        }

        [Test]
        public void ASyntaxErrorKeepsThePosition()
        {
            var turn = new Protocol(NewGame()).Evaluate("if ( 1 + true ) { return 1 } else { return 2 }");

            Assert.That(turn.Ok, Is.False);
            Assert.That(turn.Error, Does.Contain("operator [+] is not defined"));
        }

        [Test]
        public void AFullPolicySolvesTheFirstLevel()
        {
            var game = NewGame();

            // a policy with no state of its own : push right while the right square is a box on the player's row
            var policy = "if ( BOX_AT( right ) && ! WALL( px + 2 , py ) ) { return right } else { return down }";

            for (var step = 0 ; step < 10 && !game.Solved ; step++)
            {
                var turn = new Protocol(game).Evaluate(policy);

                Assert.That(turn.Ok, Is.True, turn.Error);

                if (!game.Move(turn.Direction)) break;
            }

            Assert.That(game.Solved, Is.True);
        }
    }
}
