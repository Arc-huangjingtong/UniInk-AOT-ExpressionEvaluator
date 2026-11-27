# Type rules

## Promotion ( C# style )

| Left / right | Result |
|---|---|
| `int` with `int` | `int` |
| any with `single`, nothing wider | `single` |
| any with `double` | `double` |
| `char` with `char` | `int` |
| `char` with numeric | the numeric type ( char promotes to int ) |

```
1 + 1.5      -> double 2.5
1 + 1.5f     -> single 2.5
1.5f + 2.5   -> double 4
'a' + 1      -> int 98
1 == 1.0     -> true
```

## Division

```
7 / 2        -> int 3       ( integer division )
7 / 2.0      -> double 3.5
1 / 0        -> InkSyntaxException "division by zero"
```

## No implicit conversion for anything else

- `bool`, `string` and `object` never take part in promotion : `1 + true` throws
- string concatenation joins two strings only : `"a" + "b"` works , `"a" + 1` throws
- integer overflow is not checked ( it is the runtime arithmetic )
- comparison needs compatible types : `1 < true` throws

## Reading a value

| Getter | Requires |
|---|---|
| `GetResult_Int()` | `TypeCode.Int32` |
| `GetResult_Bool()` | `TypeCode.Boolean` |
| `GetResult_Char()` | `TypeCode.Char` |
| `GetResult_Float()` | `TypeCode.Single` |
| `GetResult_Double()` | `TypeCode.Double` |
| `GetResult_String()` | `TypeCode.String` |
| `GetResult_Object()` | `TypeCode.Object` |

A wrong getter throws `InkSyntaxException` ( "the value type is not [ X ] but [ Y ]" ) and does **not**
release the value , so release it yourself in that case.
