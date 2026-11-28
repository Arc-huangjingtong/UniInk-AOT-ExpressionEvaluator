<div style="text-align:center">
  <img src="Arc.UniInk.Assets/UniInk_ReadmeHeader.jpg" alt="UniInk Header" style="width:100%;height:auto;"/>
</div>

# ✒️ UniInk - AOT Expression Evaluator

> AOT-friendly, zero-allocation C#-style **expression & mini-script evaluator** for Unity IL2CPP and .NET.

![Stars Num](https://img.shields.io/github/stars/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator?style=social&logo=github)
![Forks Num](https://img.shields.io/github/forks/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator?style=social&logo=github)
![License  ](https://img.shields.io/badge/license-MIT-yellow)
![Version  ](https://img.shields.io/badge/version-1.4.0-green)
![C#       ](https://img.shields.io/badge/CSharp-9.0%2B-blue)
![Platforms](https://img.shields.io/badge/platforms-Android%20|%20Windows%20|%20(ios)-lightgrey)
---

# 😀Start

> Targets `netstandard2.0` and `netstandard2.1` — one package , consumed by .NET Framework 4.6.1+ , .NET Core 3.0+ / .NET 5-10 , and Unity ( the 2.0 build also covers Unity versions older than 2021.2 ) .

### Option A : NuGet ( .NET )

```bash
dotnet add package Arc.UniInk
```

### Option B : Unity Package Manager ( Unity 2021.3+ )

`Window > Package Manager > + > Add package from git URL` :

```
https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator.git?path=Unity/Packages/com.arc.uniink
```

The package ships the sources ( not a dll ) plus a sample : **Window > Package Manager > UniInk > Samples > Import** .

Offline / air-gapped ? Download `com.arc.uniink-<version>.tgz` from the
[releases](https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator/releases) and use
`Window > Package Manager > + > Add package from tarball...` .

### Option B2 : classic `.unitypackage` ( Unity 2019+ )

No package manager needed : grab `Arc.UniInk-<version>.unitypackage` from the releases and double click it
( or `Assets > Import Package > Custom Package...` ) . It imports into `Assets/Arc.UniInk/` and works on any
Unity version that can compile C# 9 , i.e. 2021.3+ for the current sources .
Pick **either** this or the UPM package — importing both would compile the same classes twice .

### Option C : Download [UniInk.cs](https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator/blob/main/Arc.UniInk/UniInk.cs) is Enough !

> Optional : also grab [UniInk.Extensions.cs](https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator/blob/main/Arc.UniInk/UniInk.Extensions.cs) if you need the `if` statement support (or want to extend the interpreter yourself).

Both files live in the `Arc.UniInk` namespace, so remember to add `using Arc.UniInk;`.

# 📝RoadMap

> The items below come from suggestions in the issues — open one if you miss something.

**Done**

- [x] Readable single-file implementation , 200+ test cases and CI
- [x] Thread-safe evaluation ( per-thread context , no shared runtime state )
- [x] NuGet package ( `Arc.UniInk` ) , `netstandard2.0` + `netstandard2.1`
- [x] C# style numeric promotion , one `InkSyntaxException` with the failing position
- [x] `if / else` statement support ( `UniInk.Extensions.cs` )
- [x] Zero boxing and zero allocation on the common path

**Next**

- [ ] Zero-allocation regression tests in CI
- [ ] Move the tests out of `Arc.UniInk.Benchmark` into their own project
- [ ] Unity package ( UPM ) support

**Maybe**

- [ ] `while` loops , arrays / indexers , string interpolation
- [ ] Serializable functions
- [ ] More numeric types ( `decimal` )


# ✨Features

- ### [UniInk.cs](Arc.UniInk/UniInk.cs) is a single file, you can download/copy the file and put it in your project.
- ### You can easy to extend the operator and function and more,because the code is clear and simple( only about 2600 lines )
- ### The performance is very good, the GC is 0 and the boxing is 0.

| Method                               | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0     | Gen1    | Allocated | Alloc Ratio |
|------------------------------------- |----------:|---------:|---------:|------:|--------:|---------:|--------:|----------:|------------:|
| TEST_Arithmetic__UniInk              | 131.93 us | 2.378 us | 2.224 us |  1.00 |    0.00 |        - |       - |         - |          NA |
| TEST_Arithmetic__UniInk_Compiled     |  89.50 us | 1.776 us | 2.309 us |  0.68 |    0.02 |        - |       - |         - |          NA |
| TEST_Arithmetic__ExpressionEvaluator | 648.03 us | 6.185 us | 4.829 us |  4.94 |    0.07 | 259.7656 |  3.9063 | 2175124 B |          NA |
| TEST_Arithmetic__Sprache             | 723.05 us | 7.927 us | 7.027 us |  5.49 |    0.12 | 529.2969 | 94.7266 | 4423541 B |          NA |
| TEST_Arithmetic__ParsecSharp         | 256.61 us | 3.342 us | 2.791 us |  1.95 |    0.04 | 201.6602 |  0.9766 | 1686898 B |          NA |
| TEST_IfStatement__UniInk             | 135.97 us | 2.649 us | 2.348 us |  1.03 |    0.02 |        - |       - |         - |          NA |

`TEST_IfStatement__UniInk` runs the same four arithmetic expressions, each wrapped in its own `if` statement, so the
two rows are directly comparable : the `if` machinery costs about 3% on top of the arithmetic work.

summary : UniInk is fast , and UniInk support more features. would you like to try it?

## 🧵 Threading

- ### Each thread owns its own evaluation context ( object pools + temp state ) , threads never share mutable runtime state
- ### Both models are supported
```csharp
// A. one interpreter per thread
Parallel.For(0, 8, _ =>
{
    var ink = new UniInk();
    ink.Evaluate("1 + 2 * 3");
});

// B. one interpreter shared by several threads
var shared = new UniInk();
shared.RegisterFunction("SUM", new InkFunction(prms => InkValue.GetIntValue(
    (int)(InkValue)prms[0] + (int)(InkValue)prms[1] + (int)(InkValue)prms[2])));

Parallel.For(0, 8, _ => shared.Evaluate("SUM(1, 2, 3) + 1"));
```
- ### Constraints
  - Register functions / variables **before** evaluating : registration itself is not synchronized
  - Functions and variables belong to the instance that registered them ( there is no global function table )
  - A registered getter / setter , and any plain `object` you store in an `InkValue` , stay your own responsibility
  - `var` variables ( `var a = 1` ) live for one `Evaluate` call only
- ### A registered function may evaluate again ( reentrant call ) on the same instance
```csharp
ink.RegisterFunction("NEST", new InkFunction(_ => ink.Evaluate("1 + 1")));
ink.Evaluate("NEST(1) + 3"); // 5
```
- ### `UniInk.ClearCache()` clears the pools of the **calling thread**
- ### The `InkValue` returned by `Evaluate` must still be released ( `GetResult_*` does it for you )


## 🔢 Type rules

- ### Numeric promotion is implicit , C# style : `int` -> `single` -> `double` , and `char` promotes to `int`
```csharp
ink.Evaluate("1 + 1.5");       // double 2.5
ink.Evaluate("1 + 1.5f");      // single 2.5
ink.Evaluate("1.5f + 2.5");    // double 4
ink.Evaluate("'a' + 1");       // int 98
ink.Evaluate("1 == 1.0");      // true
ink.Evaluate("-1.5");          // double -1.5
```
- ### Integer division stays integer
```csharp
ink.Evaluate("7 / 2");         // int 3
ink.Evaluate("7 / 2.0");       // double 3.5
```
- ### No implicit conversion for the other types
  - `bool` / `string` / `object` never take part in the promotion : `1 + true` throws `InkSyntaxException`
  - string concatenation only joins two strings : `"a" + "b"` works , `"a" + 1` throws
  - integer overflow is not checked ( the arithmetic is the runtime one )


## ❗ Errors

Every syntax or evaluation failure throws `InkSyntaxException`, and the exception carries where it happened :

```csharp
try
{
    ink.Evaluate("1 + true");
}
catch (InkSyntaxException exception)
{
    Console.WriteLine(exception.Message);  // operator [+] is not defined for [Int32] and [Boolean] at 2 in "1 + true"
    Console.WriteLine(exception.Position); // 2   ( zero based character index , -1 when unknown )
    Console.WriteLine(exception.Expression); // 1 + true
}
```

`exception.ToString()` prints the message , the expression , and a caret under the failing character :

```
InkSyntaxException: operator [+] is not defined for [Int32] and [Boolean] at 2 in "1 + true"
1 + true
  ^
```

Division by zero , an unknown identifier , a duplicate variable declaration , a malformed `if` statement and every other evaluation failure use the same type .

## Evaluate Expression

- ### ✨Support Arithmetic Operations ( Easy extend Operator )

```csharp

// 1. Create a new instance of the interpreter 
var ink = new UniInk();
// 2. Evaluate the expression , it always returns an InkValue
var res1 = ink.Evaluate("+9 * (1+1 + 1 + 1 + 1+1+1)");
// 3. Get the result with the type you want, the result is 63
Console.WriteLine(res1.ValueInt); 

// PS : the process of calculation is 0 GC and 0 Boxing
//      if you want improve the performance further , recycle the InkValue

InkValue.Release(res1); // now the GC is 0 completely!

```

Other example

```csharp

var ink = new UniInk();

// float type : GetResult_Float() returns the value and recycles the InkValue for you
var res = ink.Evaluate("3333333.3333333f-3.3f+3.3f+  3.3f - 3.3f").GetResult_Float();
Console.WriteLine(res);

// double type
var res2 = ink.Evaluate("+123456789.987654321d + 987654321.123456789d").GetResult_Double();
Console.WriteLine(res2);

```

- ### ✨Support Logical Operations ( Easy extend Operator )

```csharp

var ink = new UniInk();

// bool type
var res = ink.Evaluate("1 < 2 || 2 ==1 || 2 < 1").GetResult_Bool();
Console.WriteLine(res); // true

```

## Evaluate Scripts

- ### ✨Support Variable Assignment

```csharp

var ink = new UniInk();

// the result will return the last expression
var res = ink.Evaluate("var aaa= 123 ;  var bbb =aaa + 1 ; aaa + bbb ").GetResult_Int();
Console.WriteLine(res); // 247

```

- ### ✨Support Function Call

```csharp

var ink = new UniInk();

// has API to register the function such as CRE ( RegisterFunction is an instance API )
ink.RegisterFunction("CRE", new InkFunction(prms =>
{
    var param1 = (InkValue)prms[0];
    var param2 = (InkValue)prms[1];

    // the return value can be an InkValue , or any plain object ( it will be wrapped automatically )
    return new Card() { ID = param1, Rarity = param2 };
}));

// the result will return the last expression
var res = ink.Evaluate("var a = CRE(1,3) ;   GET(a, Rarity) == 3 ").GetResult_Bool();


```

- ### ✨Support Lambda Expression

```csharp

var ink = new UniInk();

// this may be complex, but we also support it
// ( FLT / GET / Config are your own registered function and variable )
var res = ink.Evaluate("FLT(Config,var c => GET(c, Rarity) == 2 && GET(c, ID) == 1)").GetResult_Object();

```

- ### ✨Support Property Getter

```csharp

var ink = new UniInk();

// use register variable to set getter
ink.RegisterVariable("grower", InkValue.SetGetter(InkValue.GetIntValue(0), value =>
{
    value.ValueType = TypeCode.Int32;
    value.ValueInt = grower++; // grower is a member variable 
}));

// the result is different every time
var res = ink.Evaluate("grower + 10086").GetResult_Int();


```

- ### 🤖 Agent Skill

The package ships a skill for AI coding agents ( `skills/uniink/SKILL.md` , the
[Agent Skills](https://agentskills.io/) format ) :

- **NuGet** : it lives inside the package under `skills/uniink/` , so it is in your NuGet cache after
  `dotnet restore` ( `dotnet-inspect package Arc.UniInk -S Skills` lists it )
- **Unity** : `Window > Package Manager > UniInk > Samples > Agent Skill ( AI ) > Import` , or import it
  from the classic `.unitypackage` ( `Assets/Arc.UniInk/Skills/` )

The skill follows the [Agent Skills](https://agentskills.io/specification) layout and lives in the shared
`.agents/skills/` tree ( `.github/skills` is a symlink to it , so Copilot / VS Code find it too ) :

```
.agents/skills/uniink/
├── SKILL.md                 entry : the two rules , a decision table , the capability matrix
├── references/              syntax , types , errors , interop and tested recipes ( loaded on demand )
└── assets/UniInkSetup.cs    a copy-paste MonoBehaviour
```

`AGENTS.md` at the repository root points Codex / Cursor style clients at the same skills .

It explains the syntax that parses , the type rules , the error model , the release contract of the pooled
`InkValue` and how to extend the evaluator — so an agent writes working expressions instead of guessing , and
knows what the language does **not** support . Every expression in `references/recipes.md` is checked against the
real library .

- ### ✨Support Property Setter

An assignment ( `a = 3` ) notifies the owner of the variable , so the value can be mirrored into your own field :

```csharp
var ink = new UniInk();

var health = 100;

var cell = InkValue.GetIntValue(0);

InkValue.SetGetter(cell, value =>
{
    value.ValueType = TypeCode.Int32;
    value.ValueInt  = health;
});
InkValue.SetSetter(cell, value => health = value.ValueInt);

ink.RegisterVariable("hp", cell);

ink.Evaluate("hp = hp - 10");   // the setter writes 90 back into health
```

Or register the cell and the setter in one call :

```csharp
ink.RegisterVariable("hp", InkValue.GetIntValue(health), value => health = value.ValueInt);
```

- the setter runs **after** the value has been written and receives the cell ( read `ValueType` / `ValueInt` / `ValueObject` )
- it runs once per assignment , never when the variable is only read
- a getter runs every time the variable is **referenced** ( `hp = hp - 10` reads it for the right side )
- without a setter an assignment still works , the value just stays inside the cell

- ### ✨Support Control Flow(If Statement)


```csharp

var ink = new UniInk();

//Not Support Nested call now like : if ( xxx ) { if ( ) { }  }
var script= @" if ( 1 > 2 )      
               {                 
                  123            
               }                 
               else if ( 3 > 6 ) 
               {                 
                  456            
               }                 
               else if ( 3 < 6 ) 
               {                 
                 return 456      
               }                 
               else              
               {                 
                 return 666      
               }                 ";

// you should copy the UniInk.Extensions.cs (Arc.UniInk/UniInk.Extensions.cs) to your project
// it provides Evaluate_IfStatement / EvaluateInt / EvaluateBool , and may enlighten you to write the custom extension
var res = ink.Evaluate_IfStatement(script) as InkValue;
Console.WriteLine(res.ValueInt); // 456

// or simply :
// var res2 = ink.EvaluateInt(script);

```


# 📋Changelog

See [CHANGELOG.md](CHANGELOG.md) for the released versions.

# 🎮Samples

- [Arc.UniInk.Sample.Sokoban](Arc.UniInk.Sample.Sokoban/README.md) — a Sokoban window where **one UniInk
  expression per turn** controls the player . The board is exposed as variables and functions , so a human or an
  AI agent can play by writing a policy string . Includes a BFS solver that proves every level is solvable .

# 💬Support

If you have any questions, suggestions, or need assistance, please feel free
to [open an issue](https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator/issues) on the UniInk repository.
We're here to help! 😊

Let's script with UniInk and unlock the full potential of your projects! 🚀💡🌟
