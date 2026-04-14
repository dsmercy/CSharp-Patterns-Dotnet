# Level 7 — Proxy Pattern

**Category:** Structural  
**Ecommerce use case:** Add caching, lazy loading, and access control in front of a product catalogue service without touching its source

---

## The Problem

Every call to `RealProductCatalogueService.GetProductAsync` hits the external API — even the same product 5 times in a row. The heavy constructor runs at startup even when no product is ever needed. Any user — including guests — can call `UpdatePriceAsync` with no rejection.

**See it live:**

```bash
cd Problem
dotnet run
```

You will see 5 `[Real] External API call` lines for the same product ID, the heavy init firing at startup, and a guest successfully updating a price.

---

## The Pattern

**Intent:** Provide a surrogate or placeholder for another object to control access to it.

**Three proxy variants demonstrated:**

| Proxy | Purpose |
|-------|---------|
| `CachingProxy` | Returns cached results on repeat requests; invalidates on write |
| `LazyLoadingProxy` | Defers expensive constructor until first method call |
| `ProtectionProxy` | Rejects write operations unless caller has `Admin` role |

**Participants:**

| Role | Type |
|------|------|
| Subject interface | `IProductCatalogueService` |
| Real Subject | `RealProductCatalogueService` |
| Proxies | `CachingProxy`, `LazyLoadingProxy`, `ProtectionProxy` |

---

## Run the Solution

```bash
cd App
dotnet run
```

Change `Demo:UserRole` in `appsettings.json` to `Admin` to see `UpdatePriceAsync` succeed through the Protection Proxy.

---

## Key Teaching Moments

- **Proxy vs Decorator (Level 5)** — the code structure is identical (same interface, wraps inner). The distinction is *intent*: Proxy **controls access** (caching, lazy init, permissions); Decorator **adds behaviour** (logging, metrics, validation). In practice you pick the name that matches your intent.

- **Caching Proxy = ASP.NET Core IMemoryCache** — every ASP.NET Core app using `IMemoryCache` is using this pattern. The check-cache-first, delegate-on-miss, store-result flow is identical.

- **LazyLoadingProxy uses `Lazy<T>`** — the same mechanism from Level 0 (Singleton). `Lazy<T>` guarantees thread-safe, one-time initialisation; the real service constructor fires once on the first call.

- **Protection Proxy separates concerns** — `RealProductCatalogueService` has zero role-checking logic. The access control rule lives in one place (`ProtectionProxy`) and is independently testable.

- **Proxies compose** — `Protection(Caching(Real))` stacks both concerns. The composition order matters: putting Protection outside means the auth gate fires before the cache is consulted for writes.

- **`DispatchProxy` in .NET** — for fully dynamic proxy creation without writing a concrete class, use `System.Reflection.DispatchProxy`. Useful for AOP (aspect-oriented programming) and mock generation.

---

## Files

```
level_7_proxy/
├── Problem/
│   ├── Problem.csproj
│   └── Program.cs                        ← Run first: 5 API calls, eager init, guest write
├── App/
│   ├── App.csproj
│   ├── Program.cs                        ← All 3 proxies + composed stack
│   ├── IProductCatalogueService.cs       ← Subject interface
│   ├── RealProductCatalogueService.cs    ← Real Subject
│   ├── Models/
│   │   └── Product.cs
│   ├── Proxies/
│   │   ├── CachingProxy.cs
│   │   ├── LazyLoadingProxy.cs
│   │   └── ProtectionProxy.cs
│   ├── appsettings.example.json
│   └── appsettings.json                  ← gitignored; set UserRole: Guest|Admin
├── .gitignore
└── README.md
```
