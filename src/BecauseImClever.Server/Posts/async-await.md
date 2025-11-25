---
title: Mastering Async/Await
summary: Common pitfalls and best practices for asynchronous programming in C#.
date: 2025-11-01
tags: [csharp, async, performance]
---

# Mastering Async/Await

Asynchronous programming is essential for responsive applications, but it's easy to get wrong.

## Best Practices

1.  **Async All the Way**: Don't block on async code with `.Result` or `.Wait()`.
2.  **Configure Await**: Use `.ConfigureAwait(false)` in library code to avoid context capturing.
3.  **ValueTask**: Consider `ValueTask` for hot paths where allocation matters.

```csharp
public async Task<User> GetUserAsync(int id)
{
    // Good
    return await _repo.GetByIdAsync(id);
}
```
