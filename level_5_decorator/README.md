# Level 5 — Decorator Pattern

**Category:** Structural  
**Ecommerce use case:** Stack logging, validation, and metrics onto order processing without subclassing

---

## The Problem

Logging and validation need to be added to `BaseOrderProcessor`. The first instinct is to subclass — but with 3 cross-cutting concerns you need up to 7 subclasses to cover every combination. Each one duplicates the delegation boilerplate and none are reusable independently across different processor types.

```
BaseOrderProcessor
├── LoggingProcessor
├── ValidatingProcessor
├── MetricsProcessor
├── LoggingValidatingProcessor        ← duplicates logging + validation
├── LoggingMetricsProcessor           ← duplicates logging + metrics
├── ValidatingMetricsProcessor        ← duplicates validation + metrics
└── LoggingValidatingMetricsProcessor ← duplicates all three
```

**See it live:**

```bash
cd Problem
dotnet run
```

---

## The Pattern

**Intent:** Attach additional responsibilities to an object dynamically. Decorators provide a flexible alternative to subclassing for extending functionality.

**Participants:**

| Role | Class |
|------|-------|
| Component interface | `IOrderProcessor` |
| Concrete Component | `BaseOrderProcessor` |
| Abstract Decorator | `OrderProcessorDecorator` |
| Concrete Decorator A | `LoggingOrderProcessor` |
| Concrete Decorator B | `ValidationOrderProcessor` |
| Concrete Decorator C | `MetricsOrderProcessor` |

---

## Run the Solution

```bash
cd App
dotnet run
```

Five stacks are assembled and run, showing bare, partial, full, invalid, and reordered configurations.

---

## Key Teaching Moments

- **3 classes replace 7 subclasses** — each decorator is one independent, reusable class. Adding a 4th concern (`FraudCheckOrderProcessor`) is one new class. The subclass approach would require 16 combinations (2⁴).

- **Order matters** — `Metrics(Logging(Validation(Base)))` records total wall-clock time including logging overhead. `Logging(Metrics(Validation(Base)))` logs the start/end but Metrics measures only Validation + Base. Stack 5 in the demo shows the difference.

- **Validation short-circuits** — when `ValidationOrderProcessor` rejects, it returns early without calling `_inner.ProcessAsync`. The outer `LoggingOrderProcessor` and `MetricsOrderProcessor` still fire (they wrap validation), but the Base is never reached.

- **ASP.NET Core Middleware is this pattern** — each middleware wraps the next via `RequestDelegate`. `UseLogging(UseAuthentication(UseRouting(...)))` is a decorator chain.

- **DI registration** — in ASP.NET Core, register with Scrutor: `services.Decorate<IOrderProcessor, LoggingOrderProcessor>()`. Without Scrutor: manually wrap in `Program.cs` as shown here.

---

## Files

```
level_5_decorator/
├── Problem/
│   ├── Problem.csproj
│   └── Program.cs              ← Run first: 4 subclasses duplicating delegation
├── App/
│   ├── App.csproj
│   ├── Program.cs              ← 5 stacks demonstrating composability and ordering
│   ├── IOrderProcessor.cs      ← Component interface + ProcessingResult
│   ├── Models/
│   │   └── Order.cs
│   └── Processors/
│       ├── BaseOrderProcessor.cs           ← Concrete Component
│       ├── OrderProcessorDecorator.cs      ← Abstract Base Decorator
│       ├── LoggingOrderProcessor.cs        ← Decorator A
│       ├── ValidationOrderProcessor.cs     ← Decorator B
│       └── MetricsOrderProcessor.cs        ← Decorator C
├── .gitignore
└── README.md
```
