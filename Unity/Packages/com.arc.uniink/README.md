# UniInk - AOT Expression Evaluator

AOT-friendly, zero-allocation C#-style **expression & mini-script evaluator** for Unity IL2CPP.

- no reflection, no `Reflection.Emit` — works on IL2CPP
- zero boxing and zero allocation per evaluation on the common path
- one `InkSyntaxException` for every failure, with the failing position

## Install

**Package Manager ( git )**

`Window > Package Manager > + > Add package from git URL` :

```
https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator.git?path=Unity/Packages/com.arc.uniink
```

**OpenUPM** ( once published )

```bash
openupm add com.arc.uniink
```

**Manual** : copy `Runtime/UniInk.cs` ( and `Runtime/UniInk.Extensions.cs` if you need `if` )
into `Assets/`. Both files live in the `Arc.UniInk` namespace.

## Usage

```csharp
using Arc.UniInk;

var ink = new UniInk();

var value = ink.Evaluate("1 + 2 * 3");   // you own this InkValue
var answer = value.GetResult_Int();      // reads it and returns it to the pool
```

```csharp
// typed helpers ( UniInk.Extensions.cs ) release the value internally
var result = ink.EvaluateInt("if ( 1 < 2 ) { return 1 } else { return 2 }"); // 1
```

Read the [project README](https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator)
for the syntax, the type rules, the threading model and the error model.

## Requirements

- Unity **2021.3** or newer ( the sources use C# 9 )
- no other package dependencies

## Notes

- `InkValue` instances come from a per-thread pool . Read them with `GetResult_*` ,
  or call `InkValue.Release` , otherwise the pool drains and allocates .
- Register functions and variables **before** evaluating .
- `var` variables live for one `Evaluate` call only .
