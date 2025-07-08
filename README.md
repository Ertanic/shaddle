# About

**Shaddle** (sharp + cuddle) is a toy project aimed at implementing the KDL parser.
Due to the project being in a **WIP** state, things like multi-line strings or KDL version annotation support may be missing.
KDL version 2 is implemented by default and there are **no** plans to support version 1.

# How to use

## Parsing KDL

```csharp
var kdl = "node1 hello=world";

var document = KdlParser.Parse(kdl);
var node1 = document.GetNode("node1");
var model = new KdlHelloWorldModel((node1.GetProperty("hello") as KdlStringValue).Value);
// or
if (!KdlParser.TryParse(out var document, out var parseError))
    return;

if (!document.TryGetNode("node1", out var node1))
    return;

if (!node1.TryGetProperty("hello", out KdlStringValue? str))
    return;

var model = new KdlHelloWorldModel(str);
```

## Serialize

```csharp
var someDocument = new KdlDocument([new KdlNode("node1") /* ... */]);
var serialized = KdlSerializer.SerializeCompact(someDocument);
Console.WriteLine(serialized); // "node1" "arg1" prop1="value"{"node2"}
```

# Build

To build a project, you must use the command:

```shell
dotnet build [-c Release]
```

Tests are performed using standard tooling:

```shell
dotnet test
```

# License

No license. You are free to use the code as you wish.