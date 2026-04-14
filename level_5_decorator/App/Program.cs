// SOLUTION: Decorator pattern — each cross-cutting concern is one independent class.
// Stack them in any order, any combination, without touching BaseOrderProcessor.

using Decorator.App;
using Decorator.App.Models;
using Decorator.App.Processors;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== Decorator Pattern — Order Processor Demo ===");
Log.Information("");

var validOrder   = new Order("ORD-001", "CUST-001", 149.99m);
var invalidOrder = new Order("ORD-002", "",          -10.00m);  // missing customer, negative amount

// ─── Stack 1: Base only — no decorators ──────────────────────────────────────
Log.Information("--- Stack 1: Base only ---");
IOrderProcessor plain = new BaseOrderProcessor();
await plain.ProcessAsync(validOrder);
Log.Information("");

// ─── Stack 2: Logging only ────────────────────────────────────────────────────
Log.Information("--- Stack 2: Logging wraps Base ---");
IOrderProcessor withLogging = new LoggingOrderProcessor(
                                  new BaseOrderProcessor());
await withLogging.ProcessAsync(validOrder);
Log.Information("");

// ─── Stack 3: All three — Metrics > Logging > Validation > Base ──────────────
// PATTERN CONCEPT: order matters.
//   - Metrics wraps everything → records total wall-clock time including logging overhead
//   - Logging wraps Validation + Base → logs after validation has already passed
//   - Validation wraps Base → rejects before touching the DB
Log.Information("--- Stack 3: Metrics > Logging > Validation > Base (valid order) ---");
IOrderProcessor fullStack = new MetricsOrderProcessor(
                                new LoggingOrderProcessor(
                                    new ValidationOrderProcessor(
                                        new BaseOrderProcessor())));
await fullStack.ProcessAsync(validOrder);
Log.Information("");

// ─── Stack 4: Same stack, invalid order — validation short-circuits ───────────
Log.Information("--- Stack 4: Same full stack, invalid order (validation rejects early) ---");
await fullStack.ProcessAsync(invalidOrder);
Log.Information("");

// ─── Stack 5: Logging > Metrics > Validation > Base ──────────────────────────
// Swap Metrics and Logging to show order changes observable behaviour:
// Metrics now measures only Validation + Base (not the logging overhead).
Log.Information("--- Stack 5: Logging > Metrics > Validation > Base (order swapped) ---");
IOrderProcessor swapped = new LoggingOrderProcessor(
                              new MetricsOrderProcessor(
                                  new ValidationOrderProcessor(
                                      new BaseOrderProcessor())));
await swapped.ProcessAsync(validOrder);
Log.Information("");

Log.Information("=== Key point ===");
Log.Information("  3 concerns, any combination: 3 classes instead of 7 subclasses.");
Log.Information("  Adding a 4th concern (e.g. FraudCheckOrderProcessor) = 1 new class.");
Log.Information("  In the Problem approach: 4 concerns would require 2^4 = 16 subclasses.");
