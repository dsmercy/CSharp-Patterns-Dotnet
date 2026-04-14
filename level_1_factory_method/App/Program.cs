// SOLUTION: Factory Method pattern — creation is centralised in one factory class
// per order type. Adding a new type = one new factory class, zero changes to callers.

using FactoryMethod.App.Factories;
using Microsoft.Extensions.Configuration;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var orderType = config["Demo:OrderType"]
    ?? throw new InvalidOperationException("Demo:OrderType not set in appsettings.json");

var customerId = config["Demo:CustomerId"] ?? "CUST-001";
var product    = config["Demo:Product"]    ?? "Sample Product";
var amount     = decimal.Parse(config["Demo:Amount"] ?? "9.99");

Log.Information("=== Factory Method Pattern — Ecommerce Order Demo ===");
Log.Information("OrderType from config: {OrderType}", orderType);
Log.Information("");

// PATTERN CONCEPT: the switch lives here ONCE — in the composition root.
// All three call sites (checkout, admin, import) ask the factory to ProcessOrder;
// none of them contain their own type-check.
OrderFactory factory = orderType switch
{
    "Digital"      => new DigitalOrderFactory(),
    "Physical"     => new PhysicalOrderFactory(),
    "Subscription" => new SubscriptionOrderFactory(),
    _ => throw new InvalidOperationException(
             $"Unknown order type '{orderType}'. Valid values: Digital, Physical, Subscription")
};

// Every call site calls the same method on OrderFactory — no type checks, no conditionals.
// Simulating three independent call sites all going through the factory:

Log.Information("--- Simulating: Checkout call site ---");
factory.ProcessOrder(customerId, product, amount);

Log.Information("");
Log.Information("--- Simulating: Admin panel call site ---");
factory.ProcessOrder("ADMIN-CUST", product, amount * 0.9m);   // 10% admin discount

Log.Information("");
Log.Information("--- Simulating: Nightly import job call site ---");
factory.ProcessOrder("IMPORT-CUST", product, amount);

Log.Information("");
Log.Information("=== Key point ===");
Log.Information("Adding a new order type requires:");
Log.Information("  1. A new Order subclass  (e.g. BulkOrder.cs)");
Log.Information("  2. A new factory class   (e.g. BulkOrderFactory.cs)");
Log.Information("  3. One new case in the switch above — in ONE place, not scattered.");
Log.Information("     All existing call sites are untouched.");
