using System;
using System.Collections.Generic;
using Arc.UniInk;
using Ink = Arc.UniInk.UniInk;   // the namespace and the class share the name

namespace Arc.UniInk.Sample.Sokoban.Core
{
    /// <summary> What one policy evaluation produced </summary>
    public readonly struct TurnResult
    {
        private TurnResult(bool ok, Direction direction, string error)
        {
            Ok        = ok;
            Direction = direction;
            Error     = error;
        }

        public bool Ok { get; }

        public Direction Direction { get; }

        public string Error { get; }

        public static TurnResult Move(Direction direction) => new(true, direction, null);

        public static TurnResult Fail(string error) => new(false, Direction.Up, error);
    }

    /// <summary>
    /// The language the game exposes to a policy . The board is readable through functions , the policy answers
    /// with a direction code , and one evaluation runs per turn — so a policy never needs a loop .
    /// </summary>
    /// <remarks>
    /// Variables   px , py , steps , pushes , solved , up , right , down , left
    /// Functions   WALL( x , y ) , BOX( x , y ) , GOAL( x , y ) , FREE( x , y )
    ///             CAN_MOVE( d ) , BOX_AT( d ) , GOAL_AT( d )
    /// </remarks>
    public sealed class Protocol
    {
        private readonly Game game;

        public Protocol(Game game) => this.game = game;

        /// <summary> A fresh interpreter wired to the current game state </summary>
        public Ink CreateInk()
        {
            var ink = new Ink();

            ink.RegisterVariable("px"    , InkValue.GetIntValue(game.PlayerX));
            ink.RegisterVariable("py"    , InkValue.GetIntValue(game.PlayerY));
            ink.RegisterVariable("steps" , InkValue.GetIntValue(game.Steps));
            ink.RegisterVariable("pushes", InkValue.GetIntValue(game.Pushes));
            ink.RegisterVariable("solved", InkValue.GetIntValue(game.Solved ? 1 : 0));

            ink.RegisterVariable("up"   , InkValue.GetIntValue((int)Direction.Up));
            ink.RegisterVariable("right", InkValue.GetIntValue((int)Direction.Right));
            ink.RegisterVariable("down" , InkValue.GetIntValue((int)Direction.Down));
            ink.RegisterVariable("left" , InkValue.GetIntValue((int)Direction.Left));

            ink.RegisterFunction("WALL"    , new InkFunction(args => Bool(game.IsWall(Int(args, 0), Int(args, 1)))));
            ink.RegisterFunction("BOX"     , new InkFunction(args => Bool(game.HasBox(Int(args, 0), Int(args, 1)))));
            ink.RegisterFunction("GOAL"    , new InkFunction(args => Bool(game.IsGoal(Int(args, 0), Int(args, 1)))));
            ink.RegisterFunction("FREE"    , new InkFunction(args => Bool(IsFree(Int(args, 0), Int(args, 1)))));
            ink.RegisterFunction("CAN_MOVE", new InkFunction(args => Bool(game.CanMove(DirectionOf(args)))));
            ink.RegisterFunction("BOX_AT"  , new InkFunction(args => Bool(HasBoxAt(DirectionOf(args)))));
            ink.RegisterFunction("GOAL_AT" , new InkFunction(args => Bool(HasGoalAt(DirectionOf(args)))));

            return ink;
        }

        /// <summary> Run one turn of the policy </summary>
        public TurnResult Evaluate(string policy)
        {
            if (string.IsNullOrWhiteSpace(policy))
            {
                return TurnResult.Fail("the policy is empty");
            }

            var ink = CreateInk();

            try
            {
                // the policy may use if / else , that lives in UniInk.Extensions.cs
                var value = ink.Evaluate_IfStatement(policy) as InkValue;

                if (value is null)
                {
                    return TurnResult.Fail("the policy returned nothing");
                }

                var raw = value.GetResult_Int();

                if (raw is < 0 or > 3)
                {
                    return TurnResult.Fail($"the policy returned {raw} , expected a direction 0..3");
                }

                return TurnResult.Move((Direction)raw);
            }
            catch (InkSyntaxException exception)
            {
                return TurnResult.Fail(exception.Message);
            }
        }

        private bool IsFree(int x, int y) => !game.IsWall(x, y) && !game.HasBox(x, y);

        private bool HasBoxAt(Direction direction)
        {
            var (x, y) = Game.Step(game.PlayerX, game.PlayerY, direction);

            return game.HasBox(x, y);
        }

        private bool HasGoalAt(Direction direction)
        {
            var (x, y) = Game.Step(game.PlayerX, game.PlayerY, direction);

            return game.IsGoal(x, y);
        }

        private static int Int(List<object> args, int index) => ((InkValue)args[index]).ValueInt;

        private static Direction DirectionOf(List<object> args) => Int(args, 0) switch
        {
            0 => Direction.Up
          , 1 => Direction.Right
          , 2 => Direction.Down
          , _ => Direction.Left
        };

        private static InkValue Bool(bool value) => InkValue.GetBoolValue(value);
    }
}
