# Level 1 — Factory Method Pattern

**Category:** Creational  
**Ecommerce use case:** Create the right order type (Digital, Physical, Subscription) without scattering type-checks across every call site

---

## The Problem

Every call site that creates an order duplicates its own `switch`/`if` block. Adding a new order type — say `BulkOrder` — means finding and updating every one of those scattered conditionals. Miss one and the system silently falls through to a wrong default.

**See it live:**

```bash
cd Problem
dotnet run
```

You will see the nightly import job fail with an `InvalidOperationException` because the developer forgot to add `BulkOrder` to that switch block.

---

## The Pattern

**Intent:** Define an interface for creating an object, but let subclasses decide which class to instantiate. The factory method defers instantiation to subclasses.

**Participants:**

| Role | Class |
|------|-------|
| Abstract Creator | `OrderFactory` |
| Concrete Creator | `DigitalOrderFactory`, `PhysicalOrderFactory`, `SubscriptionOrderFactory` |
| Abstract Product | `Order` |
| Concrete Product | `DigitalOrder`, `PhysicalOrder`, `SubscriptionOrder` |

---

## Run the Solution

Change `Demo:OrderType` in `App/appsettings.json` to `Digital`, `Physical`, or `Subscription`, then:

```bash
cd App
dotnet run
```

---

## Key Teaching Moments

- **Factory Method is about subclassing** — the abstract `OrderFactory` declares `CreateOrder()`, and each concrete factory overrides it. This is not the same as a static `Create()` helper method (which is a simpler "static factory" pattern).

- **Template method built in** — `ProcessOrder()` in `OrderFactory` orchestrates the steps (create → log → process → log complete) identically for every order type. The *only* variation is which `Order` subclass `CreateOrder()` returns.

- **The switch lives in ONE place** — the composition root (`Program.cs`). All call sites call `factory.ProcessOrder(...)` with no knowledge of which concrete type will be created. Adding `BulkOrder` means one new factory class and one new `case` in the composition root — not hunting three files.

- **Open/Closed Principle** — existing factories and order classes are closed to modification. Adding a new order type is purely additive.

- **Contrast with Abstract Factory (Level 3)** — Factory Method creates *one product* via inheritance. Abstract Factory creates *families of related products* via composition.

---

## Adding a New Order Type

To add `BulkOrder`:

1. `Orders/BulkOrder.cs` — extend `Order`, override `Process()`
2. `Factories/BulkOrderFactory.cs` — extend `OrderFactory`, override `CreateOrder()`
3. Add `"Bulk" => new BulkOrderFactory()` in `Program.cs`

**Zero changes** to `DigitalOrderFactory`, `PhysicalOrderFactory`, `SubscriptionOrderFactory`, or any of the three simulated call sites.

---

## Files

```
level_1_factory_method/
├── Problem/
│   ├── Problem.csproj
│   └── Program.cs              ← Run first to see scattered switch blocks fail
├── App/
│   ├── App.csproj
│   ├── Program.cs              ← Composition root: one switch, three call sites
│   ├── Orders/
│   │   ├── Order.cs            ← Abstract Product
│   │   ├── DigitalOrder.cs
│   │   ├── PhysicalOrder.cs
│   │   └── SubscriptionOrder.cs
│   ├── Factories/
│   │   ├── OrderFactory.cs     ← Abstract Creator (factory method + template method)
│   │   ├── DigitalOrderFactory.cs
│   │   ├── PhysicalOrderFactory.cs
│   │   └── SubscriptionOrderFactory.cs
│   ├── appsettings.example.json
│   └── appsettings.json        ← gitignored; copy from example
├── .gitignore
└── README.md
```
