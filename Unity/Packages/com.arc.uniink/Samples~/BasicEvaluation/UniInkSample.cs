using System;
using Arc.UniInk;
using UnityEngine;

namespace Arc.UniInk.Samples
{
    /// <summary> Evaluate UniInk expressions from a component </summary>
    public sealed class UniInkSample : MonoBehaviour
    {
        [Tooltip("Expression to evaluate. See the README for the supported syntax.")]
        [SerializeField] private string expression = "1 + 2 * 3";

        [Tooltip("Expression evaluated as an if statement ( UniInk.Extensions.cs ).")]
        [SerializeField] private string ifStatement = "if ( 1 < 2 ) { return 1 } else { return 2 }";

        private readonly UniInk ink = new();

        private void Start()
        {
            EvaluateArithmetic();
            EvaluateIfStatement();
            ReportAnError();
        }

        /// <summary> Read the value , then return the instance to the pool </summary>
        private void EvaluateArithmetic()
        {
            var value = ink.Evaluate(expression);

            if (value is null)
            {
                Debug.LogWarning($"[UniInk] empty expression");

                return;
            }

            Debug.Log($"[UniInk] {expression} = {value.ValueInt}");

            InkValue.Release(value);
        }

        /// <summary> The typed extension evaluates an if statement and releases the value for you </summary>
        private void EvaluateIfStatement()
        {
            var result = ink.EvaluateInt(ifStatement);

            Debug.Log($"[UniInk] {ifStatement} => {result}");
        }

        /// <summary> Every failure is an InkSyntaxException carrying the position of the problem </summary>
        private void ReportAnError()
        {
            try
            {
                ink.Evaluate("1 + true");
            }
            catch (InkSyntaxException exception)
            {
                Debug.LogWarning($"[UniInk] {exception.Message} ( at {exception.Position} )");
            }
        }
    }
}
