// Copy this file into your project as a starting point.
// It shows the three things every UniInk integration needs :
// evaluate , read the result safely , and report a failure with its position.
using System;
using Arc.UniInk;
using UnityEngine;

namespace YourGame
{
    public sealed class UniInkSetup : MonoBehaviour
    {
        [Tooltip("Expression evaluated on Start. See the UniInk SKILL.md for the supported syntax.")]
        [SerializeField] private string expression = "1 + 2 * 3";

        [Tooltip("Optional if statement, evaluated with UniInk.Extensions.cs.")]
        [SerializeField] private string condition = "if ( hp < 20 ) { return 1 } else { return 0 }";

        [Tooltip("Mirrored into this field by the setter, so a script can write it back.")]
        [SerializeField] private int hp = 100;

        private readonly UniInk ink = new();

        private void Awake()
        {
            // the cell is owned by the interpreter , the setter mirrors every assignment into hp
            ink.RegisterVariable("hp", InkValue.SetSetter(InkValue.GetIntValue(hp), value => hp = value.ValueInt));
        }

        private void Start()
        {
            EvaluateToLog(expression);
            EvaluateConditionToLog(condition);

            ink.Evaluate("hp = hp - 10");
            Debug.Log($"[UniInk] the script wrote hp back : {hp}");   // 90
        }

        /// <summary> Read with a typed getter : it returns the instance to the pool for you </summary>
        private void EvaluateToLog(string source)
        {
            var value = ink.Evaluate(source);

            if (value is null)
            {
                Debug.LogWarning("[UniInk] empty expression");

                return;
            }

            try
            {
                // GetResult_Int throws when the type is not Int32 and keeps the value alive ,
                // so release it in that case instead of leaking the instance
                Debug.Log($"[UniInk] {source} = {value.GetResult_Int()}");
            }
            catch (InkSyntaxException exception)
            {
                InkValue.Release(value);
                Debug.LogWarning($"[UniInk] {exception.Message} ( at {exception.Position} )");
            }
        }

        private void EvaluateConditionToLog(string source)
        {
            try
            {
                Debug.Log($"[UniInk] {source} => {ink.EvaluateInt(source)}");
            }
            catch (InkSyntaxException exception)
            {
                Debug.LogWarning($"[UniInk] {exception.Message} ( at {exception.Position} )");
            }
        }
    }
}
