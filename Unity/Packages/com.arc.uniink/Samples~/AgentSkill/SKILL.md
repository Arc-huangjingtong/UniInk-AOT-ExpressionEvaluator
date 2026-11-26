---
name: uniink
description: Write and debug UniInk expressions and mini-scripts (the Arc.UniInk NuGet / Unity package), and integrate the evaluator into a .NET or Unity project. Use when a task mentions UniInk, Arc.UniInk, an embedded expression evaluator, or running a small expression script on IL2CPP / AOT.
license: MIT
metadata:
  package: Arc.UniInk
  repository: https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator
---

# UniInk

UniInk is a single file C#-style **expression and mini-script evaluator** built for AOT runtimes
(Unity IL2CPP and .NET). It never uses reflection, boxes nothing on the common path and allocates
nothing per evaluation, as long as the caller follows the release contract below.

It evaluates **expressions**, not a programming language. There are no loops, arrays, classes,
casts, switch or ternary operators. If a task needs those, say so instead of inventing syntax.

## Quick start

```csharp
using Arc.UniInk;

var ink = new UniInk();

var value  = ink.Evaluate("1 + 2 * 3");   // InkValue owned by the caller
var answer = value.GetResult_Int();       // 7, and the instance goes back to the pool
```

`if` statements live in `UniInk.Extensions.cs` (the same namespace) :

```csharp
var r = ink.EvaluateInt("if ( 1 < 2 ) { return 10 } else { return 20 }");   // 10
```

## The two rules people break

1. **Release what you get.** `Evaluate` / `ExecuteProcess` hand out a pooled `InkValue`. Read it with
   `GetResult_Int()` / `GetResult_Bool()` / `GetResult_Float()` / `GetResult_Double()` /
   `GetResult_Char()` / `GetResult_String()` / `GetResult_Object()` ( they release it ) or call
   `InkValue.Release(value)`. Forgetting it drains the pool and turns "zero allocation" into "allocates".
2. **The syntax is small.** Reject `while`, `for`, `foreach`, `a[0]`, `a.b`, `(int)x`, `x ? y : z`,
   `switch`, `++`, `+=`, string interpolation and `1 + true` — none of them parse.

## What is supported

| Feature | Example | Notes |
|---|---|---|
| numbers | `42`, `-42`, `1.5f`, `1.5` | int / single / double |
| char / string / bool | `'a'`, `"hi"`, `true` | `'a'` + 1 is int 98 |
| arithmetic | `1 + 2 * 3 - 4 / 2 % 3` | integer division stays integer |
| comparison | `1 < 2`, `1.5f >= 1.5f`, `'a' == 'a'` | works per type and after promotion |
| logic | `true && false || 1 + 1 == 2` | `&&` / `||` on bool only |
| assignment | `a = 1` | on a registered cell or a `var` |
| variables | `var a = 1; a + 1` | `var` lives for one `Evaluate` call |
| functions | `ink.RegisterFunction("SUM", new InkFunction(args => ...))` then `SUM(1,2,3)` | args are `InkValue` |
| lambdas | `FLT(Config, var c => GET(c, Rarity) == 2)` | only as a function argument |
| `if` / `else if` / `else` | `if ( a > 1 ) { return 1 } else { return 2 }` | body must `return` when you need a value |

## Details

- [references/syntax.md](references/syntax.md) — the grammar that actually parses, operator precedence
- [references/types.md](references/types.md) — promotion rules, what throws on mixed types
- [references/errors.md](references/errors.md) — `InkSyntaxException`, position, caret, common messages
- [references/interop.md](references/interop.md) — functions, variables, getters / setters, threading, Unity vs .NET

## Integration

**.NET** — `dotnet add package Arc.UniInk` ( `netstandard2.0` + `netstandard2.1` ).

**Unity 2021.3+** — package manager :
`https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator.git?path=Unity/Packages/com.arc.uniink`
or grab `com.arc.uniink-<version>.tgz` / `Arc.UniInk-<version>.unitypackage` from the releases page.

**Copying the source** is supported too : `UniInk.cs` ( required ) and `UniInk.Extensions.cs`
( optional, adds `if` / `else` and `EvaluateInt` / `EvaluateBool` ). Both files are in the
`Arc.UniInk` namespace.

## Never do this

- register functions with reflection or attributes (`[InkMethod]` style) — it cannot work on IL2CPP
- keep a `Predicate<object>` or an `InkValue` from a lambda argument after the same `Evaluate` call
- mutate a registered `InkValue` from your own code while another thread is evaluating
- use `InkValue` after `Release` — the instance is already back in the pool
