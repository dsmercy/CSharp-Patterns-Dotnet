# Level 8 — Strategy Pattern

**Category:** Behavioral  
**Ecommerce use case:** Plug in different shipping cost algorithms at runtime without changing the order calculator

---

## The Problem

`BadShippingCalculator.Calculate` contains a `switch` block with a case for every strategy. Adding `SameDay` shipping means opening this class, inserting new cases in two separate branches, and re-testing every existing case. Swap the wrong string and the wrong rate is silently applied to every order.

**See it live:**

```bash
cd Problem
dotnet run
```

Known strategies work, but `"SameDay"` throws at runtime with `NotSupportedException` because the developer forgot to add its case to the switch.

---

## The Pattern

**Intent:** Define a family of algorithms, encapsulate each one, and make them interchangeable. Strategy lets the algorithm vary independently from clients that use it.

**Participants:**

| Role | Type |
|------|------|
| Strategy interface | `IShippingStrategy` |
| Concrete Strategies | `StandardShippingStrategy`, `ExpressShippingStrategy`, `FreeShippingStrategy`, `SameDayShippingStrategy` |
| Context | `ShippingCalculator` |

---

## Run the Solution

Change `Demo:ShippingStrategy` in `App/appsettings.json` to `Standard`, `Express`, `Free`, or `SameDay`, then:

```bash
cd App
dotnet run
```

---

## Key Teaching Moments

- **No switch in the Context** — `ShippingCalculator` contains zero `if`/`switch` logic. Adding a 5th strategy is one new class implementing `IShippingStrategy`; the calculator is untouched.

- **Open/Closed Principle** — the system is open for extension (new strategies) and closed for modification (no existing code changes). `BadShippingCalculator` violates this; the solution enforces it.

- **Runtime swap** — `SetStrategy()` lets the active algorithm change between calls. Real checkout flows use this when a user changes their delivery option on the page.

- **Each strategy owns its own validation** — `FreeShippingStrategy` enforces the £50 minimum; `SameDayShippingStrategy` enforces the 12:00 UTC cutoff. These rules live in the strategy, not in the calculator.

- **DI registration** — in ASP.NET Core, register all strategies and resolve by name:
  ```csharp
  services.AddTransient<IShippingStrategy, StandardShippingStrategy>();
  services.AddTransient<IShippingStrategy, ExpressShippingStrategy>();
  // ...
  // Resolve: serviceProvider.GetServices<IShippingStrategy>()
  //                         .Single(s => s.StrategyName == name)
  ```

- **Real-world C# examples** — `IComparer<T>` is a sorting strategy; `IEqualityComparer<T>` is a hashing/equality strategy; ASP.NET Core authentication handlers are strategies.

---

## Files

```
level_8_strategy/
├── Problem/
│   ├── Problem.csproj
│   └── Program.cs                        ← Run first: switch block, SameDay throws
├── App/
│   ├── App.csproj
│   ├── Program.cs                        ← All strategies + threshold guard + runtime swap
│   ├── IShippingStrategy.cs              ← Strategy interface + ShippingQuote
│   ├── ShippingCalculator.cs             ← Context (zero switch statements)
│   ├── Models/
│   │   └── Order.cs
│   ├── Strategies/
│   │   ├── StandardShippingStrategy.cs
│   │   ├── ExpressShippingStrategy.cs
│   │   ├── FreeShippingStrategy.cs
│   │   └── SameDayShippingStrategy.cs    ← New strategy: one class, zero changes elsewhere
│   ├── appsettings.example.json
│   └── appsettings.json                  ← gitignored; set ShippingStrategy
├── .gitignore
└── README.md
```
