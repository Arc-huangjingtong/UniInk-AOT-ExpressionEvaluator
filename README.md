<div style="text-align:center">
  <img src="MarkDown_Assets/UniInk_ReadmeHeader.jpg" alt="UniInk Header" style="width:100%;height:auto;"/>
</div>

# ✒️ UniInk - AOT C# Interpreter

![Stars Num](https://img.shields.io/github/stars/Arc-huangjingtong/UniInk-CSharpInterpreter4AOT?style=social&logo=github)
![Forks Num](https://img.shields.io/github/forks/Arc-huangjingtong/UniInk-CSharpInterpreter4AOT?style=social&logo=github)
![License  ](https://img.shields.io/badge/license-MIT-yellow)
![Version  ](https://img.shields.io/badge/version-1.0.0-green)
![C#       ](https://img.shields.io/badge/CSharp-8.0%20+%20-blue)
![Platforms](https://img.shields.io/badge/platforms-Android%20|%20Windows%20|%20(ios)-lightgrey)
---

# 😀Start

### Download [UniInk.cs](https://github.com/Arc-huangjingtong/UniInk-CSharpInterpreter4AOT/blob/main/Arc.UniInk/Arc.UniInk/UniInk.cs) is Enough !

> Optional : also grab [UniInk.Extensions.cs](https://github.com/Arc-huangjingtong/UniInk-CSharpInterpreter4AOT/blob/main/Arc.UniInk/Arc.UniInk/UniInk.Extensions.cs) if you need the `if` statement support (or want to extend the interpreter yourself).

Both files live in the `Arc.UniInk` namespace, so remember to add `using Arc.UniInk;`.

# 📝RoadMap

- [ ] Support more features.
- [ ] Nuget Package Support.
- [ ] Unity Package Support.
- [ ] Write more test cases.
- [ ] Serializable function? Maybe interesting?
- [ ] improve the readability.
- [ ] thread safe support (or provide a thread safe way to execute many expression).

# ✨Features

- ### [UniInk.cs](Arc.UniInk/Arc.UniInk/UniInk.cs) is a single file, you can download/copy the file and put it in your project.
- ### You can easy to extend the operator and function and more,because the code is clear and simple( only about 2600 lines )
- ### The performance is very good, the GC is 0 and the boxing is 0.

| Method                                |            Mean |         Error |         StdDev |          Median |     Gen0 |    Gen1 | Allocated |
|---------------------------------------|----------------:|--------------:|---------------:|----------------:|---------:|--------:|----------:|
| TEST_Arithmetic__UniInkSpeed          |   293,666.10 ns |    333.563 ns |     260.424 ns |   293,632.98 ns |        - |       - |      68 B |
| TEST_Arithmetic__UniInkSpeed_Compiled |   175,568.29 ns |  3,454.616 ns |   5,063.732 ns |   176,105.37 ns |        - |       - |     658 B |
| TEST_Arithmetic__ExpressionEvaluator  | 2,622,376.28 ns | 10,903.378 ns |  10,199.026 ns | 2,623,418.75 ns | 390.6250 | 11.7188 | 2068334 B |
| TEST_Arithmetic__Sprache              | 2,797,690.07 ns | 91,953.442 ns | 265,306.706 ns | 2,677,421.88 ns | 535.1563 | 19.5313 | 2817177 B |
| TEST_Arithmetic__ParsecSharp          | 1,114,429.92 ns | 21,943.809 ns |  27,751.892 ns | 1,113,960.74 ns | 162.1094 |       - |  851830 B |

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

// you should copy the UniInk.Extensions.cs (Arc.UniInk/Arc.UniInk/UniInk.Extensions.cs) to your project
// it provides Evaluate_IfStatement / EvaluateInt / EvaluateBool , and may enlighten you to write the custom extension
var res = ink.Evaluate_IfStatement(script) as InkValue;
Console.WriteLine(res.ValueInt); // 456

// or simply :
// var res2 = ink.EvaluateInt(script);

```


# 💬Support

If you have any questions, suggestions, or need assistance, please feel free
to [open an issue](https://github.com/Arc-huangjingtong/UniInk-CSharpInterpreter4AOT/issues) on the UniInk repository.
We're here to help! 😊

Let's script with UniInk and unlock the full potential of your projects! 🚀💡🌟
