// SOLUTION: Command pattern — each action is an object with Execute + Undo.
//
// Teaching points demonstrated:
//   1. Undo stack — three commands executed, then reversed one by one
//   2. History log — full audit trail after every operation
//   3. Stored undo state — ApplyDiscountCommand captures _previousAmount;
//      undo is only possible because the command owns that snapshot
//   4. Invoker isolation — OrderCommandInvoker never touches Order or
//      Repository directly; it only calls IOrderCommand.ExecuteAsync / UndoAsync

using Command.App;
using Command.App.Commands;
using Command.App.Models;
using Command.App.Repositories;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
    .CreateLogger();

Log.Information("=== SOLUTION: Command pattern ===");
Log.Information("");

var repository = new OrderRepository();
var invoker    = new OrderCommandInvoker();
var order      = new Order("ORD-001", "CUST-42", 200.00m);

// ── Section 1: Execute three commands ─────────────────────────────────────────
Log.Information("--- Executing commands ---");

await invoker.ExecuteAsync(new PlaceOrderCommand(repository, order));
PrintHistory(invoker, order);

await invoker.ExecuteAsync(new ApplyDiscountCommand(repository, order, 10m));
PrintHistory(invoker, order);

await invoker.ExecuteAsync(new CancelOrderCommand(repository, order));
PrintHistory(invoker, order);

// ── Section 2: Undo all three in reverse order ────────────────────────────────
Log.Information("--- Undoing commands (LIFO) ---");

Log.Information("  Undo 1 — reverse the cancellation:");
await invoker.UndoLastAsync();
PrintHistory(invoker, order);

Log.Information("  Undo 2 — reverse the discount (wrong code):");
await invoker.UndoLastAsync();
PrintHistory(invoker, order);

Log.Information("  Undo 3 — reverse the placement:");
await invoker.UndoLastAsync();
PrintHistory(invoker, order);

Log.Information("  Undo 4 — nothing left:");
await invoker.UndoLastAsync();
Log.Information("");

// ── Notes ─────────────────────────────────────────────────────────────────────
Log.Information(">>> Commands can be serialised → basis of event sourcing.");
Log.Information(">>> Real-world: ICommand in WPF/MAUI, IRequest in MediatR,");
Log.Information(">>>             EF Core queues commands before SaveChanges.");

// ─── Helper ───────────────────────────────────────────────────────────────────

static void PrintHistory(OrderCommandInvoker invoker, Order order)
{
    var history = invoker.GetHistory().ToList();
    Log.Information("  Order state: {Order}", order);
    if (history.Count == 0)
        Log.Information("  History: (empty)");
    else
        for (int i = 0; i < history.Count; i++)
            Log.Information("  History[{I}]: {Desc}", i + 1, history[i]);
    Log.Information("");
}
