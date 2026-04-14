# Level 2 — Builder Pattern

**Category:** Creational  
**Ecommerce use case:** Construct a complex order with many optional fields without a 10-parameter constructor

---

## The Problem

`Order` requires a 10-parameter constructor — most parameters optional. Callers pass `null` for fields they don't need, positional mistakes are invisible to the compiler, and there is no single place that validates the combination.

**See it live:**

```bash
cd Problem
dotnet run
```

You will see:
- A compilable positional bug (discount % and gift message swapped) that produces a corrupted order
- A physical order missing a shipping address that blows up at dispatch time — not at construction
- A comment showing how adding an 11th field breaks every existing call site

---

## The Pattern

**Intent:** Separate the construction of a complex object from its representation. The same construction process can create different representations.

**Participants:**

| Role | Class |
|------|-------|
| Builder | `OrderBuilder` |
| Director | `OrderDirector` |
| Product | `Order` |

---

## Run the Solution

```bash
cd App
dotnet run
```

---

## Key Teaching Moments

- **Named, chainable methods** — `WithDiscount("SAVE10", 10m)` is unambiguous; `new Order(..., "SAVE10", 10m, null, ...)` is not. Positional mistakes become impossible.

- **`Build()` is the single validation gate** — `Order` has an `internal` constructor, so the only way to get an `Order` is through `OrderBuilder.Build()`. Invalid state (no items, negative discount) is caught at construction time, not at dispatch time.

- **Adding a new field is non-breaking** — adding `WithLoyaltyPoints(int)` to `OrderBuilder` requires zero changes to existing callers. Compare to the 10-param constructor where inserting a parameter at position 6 breaks every call site.

- **Director is optional** — use it when multiple callers need to follow the same construction sequence. The Director does not call `Build()` — it only sets fields. The caller always holds the builder and calls `Build()` at the end, optionally adding extra steps before or after the Director runs.

- **When C# object initialisers are enough** — `new Order { CustomerId = "x", ... }` works fine for simple objects. Reach for Builder when: validation spans multiple fields, there are multiple valid configurations with different required fields, or you want a domain-readable fluent API.

---

## Files

```
level_2_builder/
├── Problem/
│   ├── Problem.csproj
│   └── Program.cs              ← Run first: positional bug + missing field + breaking change
├── App/
│   ├── App.csproj
│   ├── Program.cs              ← Three orders: manual, director gift, director subscription
│   ├── Models/
│   │   ├── Order.cs            ← Product (internal constructor — only builder can create it)
│   │   ├── Address.cs
│   │   └── OrderItem.cs
│   ├── OrderBuilder.cs         ← Fluent builder with Build() validation
│   ├── OrderDirector.cs        ← Pre-defined construction sequences
│   └── appsettings.example.json
├── .gitignore
└── README.md
```
