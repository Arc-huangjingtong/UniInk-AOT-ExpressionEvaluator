# Basic Evaluation

1. Import this sample from **Window > Package Manager > UniInk > Samples**.
2. Add `UniInkSample` to any GameObject.
3. Press Play and open the Console.

The sample shows the three things most projects need:

- `ink.Evaluate(expression)` returns an `InkValue` that **you own** — release it with `GetResult_*` or `InkValue.Release`
- `ink.EvaluateInt(ifStatement)` from `UniInk.Extensions.cs` evaluates an `if` statement and releases the value internally
- a failure throws `InkSyntaxException`, which carries `Position` and `Expression`
