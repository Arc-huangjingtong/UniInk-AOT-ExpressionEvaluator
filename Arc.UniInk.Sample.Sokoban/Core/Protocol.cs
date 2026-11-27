using System;
using System.Collections.Generic;
using Arc.UniInk;
using Ink = Arc.UniInk.UniInk;   // the namespace and the class share the name

namespace Arc.UniInk.Sample.Sokoban.Core
{
    /// <summary> What one policy evaluation produced </summary>
    public readonly struct TurnResult
    {
        private TurnResult(bool ok, Direction direction, bool fromAction, string error)
        {
            Ok         = ok;
            Direction  = direction;
            FromAction = fromAction;
            Error      = error;
        }

        public bool Ok { get; }

        public Direction Direction { get; }

        /// <summary> True when the policy named an action getter ( <c>R</c> , <c>L</c> , <c>U</c> , <c>D</c> ) </summary>
        public bool FromAction { get; }

        public string Error { get; }

        public static TurnResult Move(Direction direction, bool fromAction) => new(true, direction, fromAction, null);

        public static TurnResult Fail(string error) => new(false, Direction.Up, false, error);
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

        /// <summary> The direction an action getter ( R / L / U / D ) asked for , the last one wins </summary>
        public Direction? Requested { get; private set; }

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

            // action getters : naming R / L / U / D asks the host to move that way once this turn ends .
            // A getter runs when the name is parsed , so the last action named in the policy wins .
            ink.RegisterVariable("U", Action(Direction.Up));
            ink.RegisterVariable("R", Action(Direction.Right));
            ink.RegisterVariable("D", Action(Direction.Down));
            ink.RegisterVariable("L", Action(Direction.Left));

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
        /// <remarks>
        /// Naming <c>R</c> , <c>L</c> , <c>U</c> or <c>D</c> records a move and the last one named wins ; the app
        /// then applies it . A policy may also return a direction code instead , which is what
        /// <see cref="TurnResult.FromAction"/> tells apart .
        /// </remarks>
        public TurnResult Evaluate(string policy)
        {
            Requested = null;

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

                if (Requested is not null)
                {
                    // an action getter already answered : the evaluated value only reports whether it is legal
                    return TurnResult.Move(Requested.Value, true);
                }

                var raw = value.GetResult_Int();

                if (raw is < 0 or > 3)
                {
                    return TurnResult.Fail($"the policy returned {raw} , expected a direction 0..3");
                }

                return TurnResult.Move((Direction)raw, false);
            }
            catch (InkSyntaxException exception)
            {
                return TurnResult.Fail(exception.Message);
            }
        }

        /// <summary> A getter cell : reading the name records the direction and reports whether it is legal </summary>
        private InkValue Action(Direction direction)
        {
            var cell = InkValue.GetIntValue(0);

            return InkValue.SetGetter(cell, value =>
            {
                Requested       = direction;
                value.ValueType = TypeCode.Int32;
                value.ValueInt  = game.CanMove(direction) ? 1 : 0;
            });
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
