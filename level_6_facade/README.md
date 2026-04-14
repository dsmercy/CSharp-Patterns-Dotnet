# Level 6 — Facade Pattern

**Category:** Structural  
**Ecommerce use case:** Hide the complexity of checkout (inventory + payment + shipping + notifications + loyalty) behind one simple `PlaceOrderAsync(cart)` call

---

## The Problem

Every endpoint that handles a checkout — web, mobile, admin — must know about five subsystems and call them in exactly the right sequence. The same 20-line orchestration block is duplicated across call sites. The mobile checkout silently omits the loyalty step — a subtle divergence impossible to catch without careful review.

**See it live:**

```bash
cd Problem
dotnet run
```

Web checkout calls all 5 steps. Mobile checkout skips loyalty. No compiler warning.

---

## The Pattern

**Intent:** Provide a simplified interface to a complex subsystem. The facade makes the common case easy while still allowing direct access to subsystems.

**Participants:**

| Role | Class |
|------|-------|
| Facade | `CheckoutFacade` |
| Subsystems | `InventoryService`, `PaymentService`, `ShippingService`, `NotificationService`, `LoyaltyService` |
| Clients | web checkout, mobile checkout, admin (all in `Program.cs`) |

---

## Run the Solution

```bash
cd App
dotnet run
```

---

## Key Teaching Moments

- **One canonical sequence** — the 5-step order lives in `CheckoutFacade.PlaceOrderAsync` once. Web, mobile, and admin call sites each reduce to one line. Adding a 6th step (fraud check) is a single edit in the Facade, picked up everywhere automatically.

- **Facade is THIN** — it coordinates and delegates; it contains no business logic. If it starts making decisions (e.g. "if total > £500 apply discount") that logic belongs in a domain service, not the Facade.

- **Facade is TRANSPARENT** — it does not lock callers out of subsystems. An admin tool that only needs to release a reservation calls `InventoryService.ReleaseAsync` directly. The Facade makes the *common* case simple; it does not prevent direct access.

- **Facade vs Proxy (Level 7)** — Facade simplifies a complex interface. Proxy controls access to an existing interface. A Facade typically wraps *multiple* objects; a Proxy wraps *one*.

- **Real-world examples** — MediatR's `IMediator.Send()` is a Facade over command/query dispatch. An API Gateway in microservices is a Facade over multiple backend services.

---

## Files

```
level_6_facade/
├── Problem/
│   ├── Problem.csproj
│   └── Program.cs                       ← Run first: duplication + missing loyalty step
├── App/
│   ├── App.csproj
│   ├── Program.cs                       ← 2 call sites + direct subsystem access
│   ├── Facade/
│   │   └── CheckoutFacade.cs            ← The Facade
│   ├── Subsystems/
│   │   ├── InventoryService.cs
│   │   ├── PaymentService.cs
│   │   ├── ShippingService.cs
│   │   ├── NotificationService.cs
│   │   └── LoyaltyService.cs
│   ├── Models/
│   │   ├── Cart.cs
│   │   ├── Address.cs
│   │   └── CheckoutResult.cs
│   └── appsettings.example.json
├── .gitignore
└── README.md
```
