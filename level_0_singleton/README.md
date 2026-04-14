# Level 0 — Singleton Pattern

**Category:** Creational  
**Ecommerce use case:** Shared database connection, single configuration reader

---

## The Problem

Every class that needs a database connection calls `new OrderDbContext()`. Run 10 parallel tasks and you get 10 separate connections opened simultaneously — exhausting the connection pool under load. Likewise, every service that reads config parses the JSON file from disk on every call.

**See it live:**

```bash
cd Problem
dotnet run
```

You will see 10 different hash codes printed — proof that 10 separate `OrderDbContext` objects were created.

---

## The Pattern

**Intent:** Ensure a class has only one instance and provide a global access point to it.

**Key mechanics in this implementation:**

| Mechanism | Why |
|-----------|-----|
| `Lazy<T>` | Thread-safe, one-time initialisation — the factory lambda runs exactly once even under parallel access |
| `sealed` class | Prevents subclassing, which could bypass the private constructor |
| `private` constructor | Stops external callers from using `new` |
| `static readonly` field | The `Lazy<T>` wrapper itself is created once at class load time |

---

## Run the Solution

```bash
cd App
dotnet run
```

Expected output shows:
- `OrderDbContext initialised` appears **once** regardless of 10 parallel tasks
- All 10 tasks print the **same hash code**
- `ConfigurationService initialised` appears **once** across three "call sites"

---

## Key Teaching Moments

- **`Lazy<T>` is the idiomatic C# Singleton** — never write double-checked locking manually (`if (_instance == null) { lock ... { if (_instance == null) ... } }`). It is verbose, easy to get wrong, and `Lazy<T>` already does it correctly.

- **`sealed` matters** — a subclass could introduce a `public` constructor, leaking instances around the private constructor guard.

- **Singleton ≠ global variable** — it controls *how many* instances exist, not *where* they are used. Overusing it couples code to a specific implementation.

- **Testing pain** — Singletons carry state between tests. In a test suite, `OrderDbContext.Instance` from test A leaks into test B. Prefer DI registration (`services.AddSingleton<OrderDbContext>()`) in production ASP.NET Core apps so the container manages the lifetime and tests can swap the registration.

- **When NOT to use it** — if you just want to avoid passing a dependency around, reach for constructor injection instead. Singleton is the right choice only when *one shared instance is a hard requirement* (e.g., connection pools, caches, hardware handles).

---

## Anti-pattern Shown in Problem/

```csharp
// BAD — not thread-safe
private static OrderDbContext? _instance;

public static OrderDbContext Instance
{
    get
    {
        if (_instance == null)          // Thread A and Thread B can both pass here
            _instance = new OrderDbContext(); // Both create an instance — race condition
        return _instance;
    }
}
```

Under parallel load two threads can both observe `_instance == null` and each construct a new object before either assigns the field.

---

## Files

```
level_0_singleton/
├── Problem/
│   ├── Problem.csproj
│   └── Program.cs          ← Run this first to see the problem
├── App/
│   ├── App.csproj
│   ├── Program.cs          ← Entry point for the solution
│   ├── Order.cs            ← Shared domain model
│   ├── OrderDbContext.cs   ← Singleton via Lazy<T>
│   ├── ConfigurationService.cs  ← Singleton backed by IConfiguration
│   ├── appsettings.example.json
│   └── appsettings.json    ← gitignored; copy from example
├── .gitignore
└── README.md
```
