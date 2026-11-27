# Recipes

Each recipe is a complete , tested expression. Variables and functions in **bold** are registered by you.

## Damage with a configurable formula

```csharp
ink.RegisterVariable("atk", InkValue.GetIntValue(10));     // a cell you update as the game runs
ink.Evaluate("atk * 2 + 5");                       // -> int
```

```csharp
// percentage scaling : keep the arithmetic in one type
ink.Evaluate("atk * 1.5f");                        // single
ink.Evaluate("atk * 1.5");                         // double
```

## Gating a rule

```csharp
var allowed = ink.EvaluateInt("if ( hp < 20 ) { return 1 } else { return 0 }") == 1;

var tier = ink.EvaluateInt(
    "if ( score > 900 ) { return 3 } else if ( score > 500 ) { return 2 } else { return 1 }");
```

## Boolean guard over registered values

```csharp
ink.RegisterVariable("hp",     InkValue.GetIntValue(100));
ink.RegisterVariable("shield", InkValue.GetIntValue(0));

var alive = ink.Evaluate("hp > 0 && shield >= 0");    // bool
```

## Filtering a configuration list with a lambda

The expression language has no member access , so a "property" is a registered **selector constant** that your
own function maps to a field ( the idiom used by this project's tests ) :

```csharp
ink.RegisterVariable("Rarity", InkValue.GetIntValue(5));   // selector ids , any stable numbers
ink.RegisterVariable("ID",     InkValue.GetIntValue(0));

ink.RegisterVariable("Config", InkValue.GetObjectValue(cards));

ink.RegisterFunction("GET", new InkFunction(args =>
{
    var card     = (Card)((InkValue)args[0]).ValueObject;
    var selector = ((InkValue)args[1]).ValueInt;

    return InkValue.GetIntValue(selector switch
    {
        0 => card.ID
      , 5 => card.Rarity
      , _ => 0
    });
}));

ink.RegisterFunction("FLT", new InkFunction(args =>
{
    var source = (List<Card>)((InkValue)args[0]).ValueObject;
    var filter = (Predicate<object>)((InkValue)args[1]).ValueObject;
    var hit    = new List<Card>();

    foreach (var card in source)
    {
        if (filter(card)) hit.Add(card);
    }

    return InkValue.GetObjectValue(hit);
}));

// finds the card with Rarity == 2 && ID == 1
ink.Evaluate("FLT( Config , var c => GET( c , Rarity ) == 2 && GET( c , ID ) == 1 )");
```

Rules for lambdas :

- the parameter is declared with `var` : `var c => …`
- the body is one expression , it may call your registered functions
- the predicate only lives for the duration of that `Evaluate` call , never store it

## Registering a name twice

`RegisterVariable` / `RegisterFunction` are a no-op when the name already exists — the first cell wins and no
error is raised . Build one interpreter per scope ( per scene , per service ) instead of re-registering on top of
the old value .

## Write back into your own field

```csharp
private int health = 100;

ink.RegisterVariable("hp", InkValue.SetSetter(InkValue.GetIntValue(0), value => health = value.ValueInt));

ink.Evaluate("hp = 100 - 30");     // health becomes 70
```

## Read a value that must be fresh on every use

```csharp
ink.RegisterVariable("time", InkValue.SetGetter(InkValue.GetDoubleValue(0), value =>
{
    value.ValueType   = TypeCode.Double;
    value.ValueDouble = Time.time;      // evaluated every time the name is referenced
}));

ink.Evaluate("time > 5.0");            // bool
```

## Evaluate the same constant expression many times

```csharp
var compiled = ink.CompileLexerAndFill(expression, 0, expression.Length - 1);

for (var i = 0 ; i < 1000 ; i++)
{
    var value = UniInk.ExecuteProcess(compiled);

    var r = value.GetResult_Int();     // releases the result

    InkSyntaxList.Recover(compiled);   // resets the list so the next call evaluates again
}
```

Do **not** share `compiled` between threads — each thread keeps its own pools , and the list belongs to one.

## Report an error to a designer

```csharp
try
{
    var value = ink.Evaluate(expression);

    return value.GetResult_Double();
}
catch (InkSyntaxException exception)
{
    // Message already contains the expression and the offset :
    // operator [+] is not defined for [Int32] and [Boolean] at 2 in "1 + true"
    Debug.LogWarning($"{exception.Message}\n{exception}");   // ToString adds the caret line
}
```
