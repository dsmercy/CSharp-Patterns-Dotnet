# Level 3 — Abstract Factory Pattern

**Category:** Creational  
**Ecommerce use case:** Swap entire payment provider families (Stripe ↔ PayPal) without changing business logic

---

## The Problem

`CheckoutService` instantiates payment provider objects directly with `new StripePaymentProcessor()`. Switching to PayPal means hunting through the service for every `new StripeXxx()` call. Worse — a developer accidentally wires `StripePaymentProcessor` with `PayPalWebhookHandler`, and the mismatch only surfaces at runtime when a webhook arrives with a PayPal signature that Stripe's verifier rejects.

**See it live:**

```bash
cd Problem
dotnet run
```

You will see the mixed-family runtime failure and a list of every line that must change to switch providers.

---

## The Pattern

**Intent:** Provide an interface for creating families of related or dependent objects without specifying their concrete classes.

**Participants:**

| Role | Type |
|------|------|
| Abstract Factory | `IPaymentProcessorFactory` |
| Concrete Factory A | `StripeFactory` |
| Concrete Factory B | `PayPalFactory` |
| Abstract Product A | `IPaymentProcessor` |
| Abstract Product B | `IRefundProcessor` |
| Abstract Product C | `IWebhookHandler` |
| Client | `CheckoutService` |

---

## Run the Solution

Change `Demo:PaymentProvider` in `App/appsettings.json` to `Stripe` or `PayPal`, then:

```bash
cd App
dotnet run
```

The four scenarios (checkout, refund, matching webhook, wrong-provider webhook) run identically for both providers. `CheckoutService` source code never changes.

---

## Key Teaching Moments

- **Family consistency is enforced by the type system** — it is impossible to mix a `StripePaymentProcessor` with a `PayPalWebhookHandler` through `IPaymentProcessorFactory`. The compiler guarantees every object in the service came from the same factory.

- **Client is completely decoupled** — `CheckoutService` references only interfaces (`IPaymentProcessor`, `IRefundProcessor`, `IWebhookHandler`). It contains zero `if provider == "Stripe"` conditionals. Adding `SquareFactory` requires zero changes to `CheckoutService`.

- **One switch, one place** — the `providerName switch` in `Program.cs` (the composition root) is the only conditional in the entire solution. In a real ASP.NET Core app this becomes one line: `services.AddTransient<IPaymentProcessorFactory, StripeFactory>()`.

- **Contrast with Factory Method (Level 1)** — Factory Method creates *one product* via subclass inheritance. Abstract Factory creates *a family of related products* via composition and interface injection.

- **DI mapping** — in production register as:
  ```csharp
  services.AddTransient<IPaymentProcessorFactory, StripeFactory>();
  // Swap to PayPal: services.AddTransient<IPaymentProcessorFactory, PayPalFactory>();
  ```

---

## Files

```
level_3_abstract_factory/
├── Problem/
│   ├── Problem.csproj
│   └── Program.cs                    ← Run first: mixed family + provider-switch cost
├── App/
│   ├── App.csproj
│   ├── Program.cs                    ← Composition root: one switch, all four scenarios
│   ├── CheckoutService.cs            ← Client — zero provider conditionals
│   ├── Interfaces/
│   │   ├── IPaymentProcessorFactory.cs   ← Abstract Factory
│   │   ├── IPaymentProcessor.cs
│   │   ├── IRefundProcessor.cs
│   │   └── IWebhookHandler.cs
│   ├── Stripe/
│   │   ├── StripeFactory.cs          ← Concrete Factory A
│   │   ├── StripePaymentProcessor.cs
│   │   ├── StripeRefundProcessor.cs
│   │   └── StripeWebhookHandler.cs
│   ├── PayPal/
│   │   ├── PayPalFactory.cs          ← Concrete Factory B
│   │   ├── PayPalPaymentProcessor.cs
│   │   ├── PayPalRefundProcessor.cs
│   │   └── PayPalWebhookHandler.cs
│   ├── appsettings.example.json
│   └── appsettings.json              ← gitignored; set PaymentProvider: Stripe|PayPal
├── .gitignore
└── README.md
```
