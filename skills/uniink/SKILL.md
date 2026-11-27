---
name: uniink
description: Write, review and debug UniInk expressions and mini-scripts, and integrate the Arc.UniInk evaluator into a .NET or Unity project. Use when a task mentions UniInk or Arc.UniInk, when a game or tool needs to evaluate a runtime expression / small script (config formulas, gating conditions, rule tables), or when an expression has to run on IL2CPP / AOT without reflection.
license: MIT
compatibility: .NET Standard 2.0+ or Unity 2021.3+ (C# 9, IL2CPP supported). No runtime dependencies, no network, works offline.
metadata:
  package: Arc.UniInk
  version: "1.3.6"
  min-unity: "2021.3"
  min-dotnet: "netstandard2.0"
  repository: https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator
---

# UniInk

UniInk evaluates **expressions and mini-scripts** written in a small C#-like language. One file
( `UniInk.cs` ) plus an optional extension file ( `UniInk.Extensions.cs` for `if` / `else` ). It never uses
reflection, boxes nothing on the common path and allocates nothing per evaluation — as long as the caller
follows the release contract below.

It is **not** a script engine for a whole program. No loops, arrays, classes, casts, `switch` or ternary.
When a task needs those, say so instead of inventing syntax.

## 60 second start

```csharp
using Arc.UniInk;

var ink = new UniInk();

var value  = ink.Evaluate("1 + 2 * 3");   // InkValue owned by the caller
var answer = value.GetResult_Int();       // 7 , and the instance goes back to the pool
```

```csharp
// if / else lives in UniInk.Extensions.cs ( same namespace )
var r = ink.EvaluateInt("if ( hp < 20 ) { return 1 } else { return 0 }");
```

## The two rules that break code

1. **Release what you get.** `Evaluate` and `ExecuteProcess` return a pooled `InkValue`. Read it with
   `GetResult_Int` / `_Bool` / `_Float` / `_Double` / `_Char` / `_String` / `_Object` ( they release it ) or call
   `InkValue.Release( value )` . Forgetting it drains the pool and turns "zero allocation" into "allocates".
2. **Do not write syntax the language does not have.** `while` , `for` , `a[0]` , `a.b` , `(int)x` , `x ? y : z` ,
   `switch` , `++` , `+=` , string interpolation and `1 + true` all throw `InkSyntaxException`.

## Decision table

| The task asks for… | Do this |
|---|---|
| a formula with numbers and comparison | plain `Evaluate` , see [references/syntax.md](references/syntax.md) |
| a condition with branches | `Evaluate_IfStatement` / `EvaluateInt` from `UniInk.Extensions.cs` |
| reading a game value inside the expression | `RegisterVariable` ( a cell ) , then assign to it to write back |
| a value that must refresh on each read | `InkValue.SetGetter` |
| writing an assignment back into your own field | `InkValue.SetSetter` or `RegisterVariable( name , cell , setter )` |
| a custom function | `RegisterFunction( name , new InkFunction( args => … ) )` |
| filtering a list with a lambda | register a helper like `FLT( Config , var c => GET( c , Rarity ) == 2 )` |
| evaluating the same constant expression in a loop | `CompileLexerAndFill` once + `ExecuteProcess` + `InkSyntaxList.Recover` , see [references/interop.md](references/interop.md) |
| loops , arrays , classes , casts | **not supported** — tell the user, do not fake it |
| a new operator or keyword | the operator table is a plain static list ; extend `InkOperator` and the dispatch , not the caller |

## Capability matrix

| Supported | Not supported |
|---|---|
| `int` `single` `double` `char` `string` `bool` literals | arrays , indexers , property access |
| `+ - * / %` with C# promotion , `!` , `&&` `||` | bitwise `& | ^ ~ << >>` |
| `< > <= >= == !=` per type and after promotion | ternary `?:` , `switch` , casts |
| `=` assignment , `var name = …` ( one call ) | `++` `--` `+=` `-=` |
| registered functions and variables | loops , `break` / `continue` |
| lambdas **only** as a function argument | storing or returning a lambda |
| `if` / `else if` / `else` with `{ return … }` | nested `if` inside a branch |

## Integration

| Host | How |
|---|---|
| .NET | `dotnet add package Arc.UniInk` ( `netstandard2.0` + `netstandard2.1` ) |
| Unity 2021.3+ | Package Manager → git URL `…?path=Unity/Packages/com.arc.uniink` , or the `.tgz` / `.unitypackage` from the releases |
| copy | drop `UniInk.cs` ( required ) and `UniInk.Extensions.cs` ( optional ) into the project |

Start from [assets/UniInkSetup.cs](assets/UniInkSetup.cs) — a MonoBehaviour that evaluates an expression ,
guards the result and reports `InkSyntaxException` with its position.

## Details , loaded on demand

- [references/syntax.md](references/syntax.md) — literals , operators and their priority , statements , lambdas
- [references/types.md](references/types.md) — promotion , division , what throws on mixed types , `GetResult_*`
- [references/errors.md](references/errors.md) — `InkSyntaxException` , position , caret , common messages
- [references/interop.md](references/interop.md) — functions , variables , getters / setters , threading , hosts
- [references/recipes.md](references/recipes.md) — worked examples : damage formula , gating , config filter , state write back

## Never do this

- register functions through reflection or attributes ( `[InkMethod]` style ) — it cannot work on IL2CPP
- keep a lambda argument or an `InkValue` from one `Evaluate` call alive after it returns
- mutate a registered cell from another thread while an evaluation is running
- use an `InkValue` after `Release` — the instance is already back in the pool
- call `ClearCache` ( it clears the pools of the calling thread ) in the middle of an evaluation
