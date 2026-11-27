# Interop : functions , variables , threads , hosts

## Register functions

```csharp
ink.RegisterFunction("SUM", new InkFunction(prms =>
{
    var a = (InkValue)prms[0];
    var b = (InkValue)prms[1];

    return InkValue.GetIntValue(a.ValueInt + b.ValueInt);   // return a pooled value
}));
```

- the delegate receives `List<object>` whose entries are `InkValue`
- return an `InkValue` ( it is released for you after the evaluation ) or a plain object
- a function may call `ink.Evaluate(...)` again ( reentrancy is supported )

## Register variables

```csharp
ink.RegisterVariable("hp", InkValue.GetIntValue(100));
```

The cell is owned by the table ( never released by the evaluator ). Assignment writes into it :

```csharp
var health = 100;

ink.RegisterVariable("hp", InkValue.SetGetter(InkValue.GetIntValue(0), value =>
{
    value.ValueType = TypeCode.Int32;
    value.ValueInt  = health;
}));

ink.RegisterVariable("hp", InkValue.GetIntValue(0), value => health = value.ValueInt);
```

- a **getter** runs every time the variable is referenced ( at parse time )
- a **setter** runs once per assignment , after the value is written
- `hp = hp - 10` reads twice ( the getter is idempotent in the example ) and writes once

## Threads

- each thread owns its evaluation context ( pools , temp state , frame stack )
- one instance may be shared by threads **as long as nothing is registered while an evaluation runs**
- registered getters / setters and plain `object` values are your responsibility

## Hosts

| Host | How |
|---|---|
| .NET | `dotnet add package Arc.UniInk` |
| Unity 2021.3+ | git URL `...?path=Unity/Packages/com.arc.uniink` , or the tarball / `.unitypackage` from the releases |
| copy | drop `UniInk.cs` ( and optionally `UniInk.Extensions.cs` ) into the project |

The package ships the sources , not a dll , so IL2CPP compiles and strips them like your own code.
