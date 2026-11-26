# Errors

Every failure throws `InkSyntaxException` ( namespace `Arc.UniInk` ) :

| Member | Meaning |
|---|---|
| `Message` | what failed , with `at <index> in "<expression>"` when known |
| `Position` | zero based character index in the expression , `-1` when unknown |
| `Expression` | the expression that failed |
| `ToString()` | the message plus the expression and a caret under the failing character |

```
InkSyntaxException: operator [+] is not defined for [Int32] and [Boolean] at 2 in "1 + true"
1 + true
  ^
```

## Common messages

| Message | Cause |
|---|---|
| `division by zero with [/]` | `1 / 0` or `1 % 0` |
| `the variable [a] is already declared in this expression` | two `var a` in one script |
| `unknown identifier [nope] : register the variable or function first` | name not registered |
| `operator [+] is not defined for [Int32] and [Boolean]` | no promotion for the types |
| `operator [<] cannot compare [Int32] and [Boolean]` | incompatible comparison |
| `the parenthesis is not balanced` | `( 1 + 2` |
| `the else branch is missing '{ ... }'` | `else` without a body |
| `the left side of the assignment must be a variable` | `1 = 2` |
| `the value type is not [Int32] but [Double]` | wrong `GetResult_*` |

## Catching

```csharp
try
{
    var value = ink.Evaluate(expression);

    var answer = value.GetResult_Int();
}
catch (InkSyntaxException exception)
{
    Debug.LogWarning($"{exception.Message} ( at {exception.Position} )");
}
```

Exceptions thrown by your **own** registered function or getter / setter are not wrapped — they
propagate as they are.
