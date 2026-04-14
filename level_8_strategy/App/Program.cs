// SOLUTION: Strategy pattern — each shipping algorithm is an independent class.
// ShippingCalculator contains no switch; adding SameDayShipping = one new class.

using Microsoft.Extensions.Configuration;
using Serilog;
using Strategy.App;
using Strategy.App.Models;
using Strategy.App.Strategies;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var strategyName = config["Demo:ShippingStrategy"] ?? "Standard";

Log.Information("=== Strategy Pattern — Shipping Calculator Demo ===");
Log.Information("  ShippingStrategy from config: {Name}", strategyName);
Log.Information("");

// PATTERN CONCEPT: the only switch in the codebase — one place, the composition root.
// All other code depends only on IShippingStrategy.
IShippingStrategy initial = strategyName switch
{
    "Standard" => new StandardShippingStrategy(),
    "Express"  => new ExpressShippingStrategy(),
    "Free"     => new FreeShippingStrategy(),
    "SameDay"  => new SameDayShippingStrategy(),
    _ => throw new InvalidOperationException(
             $"Unknown strategy '{strategyName}'. Valid: Standard, Express, Free, SameDay")
};

var calculator = new ShippingCalculator(initial);
var smallOrder = new Order("ORD-001", "CUST-001", 29.99m);
var largeOrder = new Order("ORD-002", "CUST-001", 149.99m);

// ─── Part 1: All strategies against the same order ────────────────────────────
Log.Information("--- Part 1: All strategies on large order (£{Amount:F2}) ---", largeOrder.Amount);

IShippingStrategy[] allStrategies =
[
    new StandardShippingStrategy(),
    new ExpressShippingStrategy(),
    new FreeShippingStrategy(),
    new SameDayShippingStrategy(),
];

foreach (var strategy in allStrategies)
{
    calculator.SetStrategy(strategy);
    try
    {
        var quote = calculator.GetQuote(largeOrder);
        Log.Information("  {Quote}", quote);
    }
    catch (InvalidOperationException ex)
    {
        Log.Warning("  {Strategy,-10} → UNAVAILABLE: {Reason}",
            strategy.StrategyName, ex.Message);
    }
}

Log.Information("");

// ─── Part 2: Free shipping threshold guard ────────────────────────────────────
Log.Information("--- Part 2: Free shipping on small order (£{Amount:F2}) — threshold guard ---",
    smallOrder.Amount);
calculator.SetStrategy(new FreeShippingStrategy());
try
{
    var quote = calculator.GetQuote(smallOrder);
    Log.Information("  {Quote}", quote);
}
catch (InvalidOperationException ex)
{
    Log.Warning("  FreeShipping  → REJECTED: {Reason}", ex.Message);
}

Log.Information("");

// ─── Part 3: Runtime swap — user changes option on checkout page ──────────────
Log.Information("--- Part 3: Runtime strategy swap (user changes delivery option) ---");
calculator.SetStrategy(new StandardShippingStrategy());
Log.Information("  Initially: {Strategy}", calculator.CurrentStrategy.StrategyName);
var q1 = calculator.GetQuote(largeOrder);
Log.Information("  Quote:     {Quote}", q1);

Log.Information("  User upgrades to Express...");
calculator.SetStrategy(new ExpressShippingStrategy());
Log.Information("  Now:       {Strategy}", calculator.CurrentStrategy.StrategyName);
var q2 = calculator.GetQuote(largeOrder);
Log.Information("  Quote:     {Quote}", q2);

Log.Information("");
Log.Information("=== Key point ===");
Log.Information("  ShippingCalculator has zero switch statements.");
Log.Information("  Adding a 5th strategy = one new class implementing IShippingStrategy.");
Log.Information("  All existing strategies and the calculator are untouched.");
