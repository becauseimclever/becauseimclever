---
title: Exploring C# 12 Features
summary: A deep dive into the new features introduced in C# 12.
date: 2025-11-20
tags: [csharp, dotnet, programming]
status: debug
---

# Exploring C# 12 Features

C# 12 introduces several exciting new features that improve developer productivity and code readability.

## Primary Constructors

You can now declare primary constructors in any `class` or `struct`.

```csharp
public class Person(string name, int age)
{
    public string Name { get; } = name;
    public int Age { get; } = age;
}
```

## Collection Expressions

Creating collections is now more concise.

```csharp
int[] numbers = [1, 2, 3, 4, 5];
List<string> names = ["Alice", "Bob", "Charlie"];
```

## Default Lambda Parameters

Lambdas can now have default parameter values.

```csharp
var add = (int x, int y = 1) => x + y;
Console.WriteLine(add(5)); // Output: 6
```
