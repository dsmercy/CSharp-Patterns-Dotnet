# Level 4 — Adapter Pattern

**Category:** Structural  
**Ecommerce use case:** Wrap a legacy XML payment gateway behind the modern `IPaymentGateway` interface the rest of the system expects

---

## The Problem

The modern codebase expects `IPaymentGateway.ChargeAsync(orderId, amount, currency)`, but the only available implementation is `LegacyPaymentGateway.ProcessXmlPayment(xmlPayload)` — a third-party class that cannot be changed. Without an adapter, every call site builds the XML payload itself, parses the XML response, and maps legacy error codes — the same boilerplate repeated in every place that touches payments.

**See it live:**

```bash
cd Problem
dotnet run
```

You will see `BadCheckoutService` duplicating XML construction and parsing across three methods, with a summary of what must change to replace the gateway.

---

## The Pattern

**Intent:** Convert the interface of a class into another interface that clients expect. Adapter lets classes work together that otherwise could not because of incompatible interfaces.

**Participants:**

| Role | Class / Type |
|------|-------------|
| Target | `IPaymentGateway` |
| Adaptee | `LegacyPaymentGateway` (cannot be changed) |
| Adapter | `LegacyPaymentGatewayAdapter` |
| Client | `CheckoutService` |

---

## Run the Solution

```bash
cd App
dotnet run
```

Scenario 3 swaps the adapter for a `StubPaymentGateway` — `CheckoutService` source is unchanged.

---

## Key Teaching Moments

- **Adapter is transparent to the client** — `CheckoutService` calls `IPaymentGateway` and has no knowledge of XML, `LegacyPaymentGateway`, or its error codes. The adapter is invisible.

- **Object Adapter vs Class Adapter** — this implementation uses **composition** (holding `LegacyPaymentGateway` as a private field). The alternative — inheriting from `LegacyPaymentGateway` (Class Adapter) — is not possible in C# when the adaptee is also implementing another interface, and is generally less flexible.

- **One place to maintain** — all XML building, response parsing, and legacy error-code mapping live in `LegacyPaymentGatewayAdapter`. The three duplicated blocks in `BadCheckoutService` collapse to one class.

- **Replaceability** — when a modern REST gateway becomes available, you write `ModernPaymentGateway : IPaymentGateway` and change one line in `Program.cs`. `CheckoutService` and every other client are untouched.

- **Real-world frequency** — the Adapter is one of the most common patterns in enterprise C#. Wrapping third-party SDKs behind your own interface is standard practice for testability and future flexibility.

---

## Files

```
level_4_adapter/
├── Problem/
│   ├── Problem.csproj
│   └── Program.cs                          ← Run first: XML duplicated across 3 call sites
├── App/
│   ├── App.csproj
│   ├── Program.cs                          ← Wires adapter; Scenario 3 shows swappability
│   ├── CheckoutService.cs                  ← Client — depends only on IPaymentGateway
│   ├── LegacyPaymentGatewayAdapter.cs      ← Adapter (Object Adapter via composition)
│   ├── Modern/
│   │   └── IPaymentGateway.cs              ← Target interface + result records
│   ├── Legacy/
│   │   └── LegacyPaymentGateway.cs         ← Adaptee (simulates unmodifiable third-party)
│   ├── Models/
│   │   └── Order.cs
│   └── appsettings.example.json
├── .gitignore
└── README.md
```
