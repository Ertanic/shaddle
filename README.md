﻿# About

**Shaddle** (sharp + cuddle) is a toy project aimed at implementing the KDL parser.
Due to the project being in a **WIP** state, things like multi-line strings or KDL version annotation support may be missing.
KDL version 2 is implemented by default and there are **no** plans to support version 1.

# How to use

Add via the `dotnet add` command:

```shell
dotnet add package Shaddle
```

or via an entry in the project file:

```xml
<ItemGroup>
    <PackageReference Include="Shaddle" Version="1.0.0" />
</ItemGroup>
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