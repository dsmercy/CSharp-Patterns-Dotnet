using Serilog;

namespace Command.App;

// PATTERN CONCEPT: Invoker.
//
// The Invoker knows only the IOrderCommand interface — it never calls
// repository or order methods directly. It:
//   - Executes a command and pushes it onto the history stack.
//   - Pops the last command from the stack to undo it.
//   - Exposes the full history as an ordered list for auditing.
//
// Real-world parallels:
//   - WPF/MAUI CommandManager
//   - MediatR pipeline with IRequest handlers
//   - EF Core's internal change tracker (queues operations before SaveChanges)
public class OrderCommandInvoker
{
    private readonly Stack<IOrderCommand> _history = new();

    public async Task ExecuteAsync(IOrderCommand command)
    {
        await command.ExecuteAsync();
        _history.Push(command);
    }

    // PATTERN CONCEPT: undo — pop and reverse the most recent command.
    public async Task UndoLastAsync()
    {
        if (!_history.TryPop(out var command))
        {
            Log.Warning("  [Invoker] Nothing to undo.");
            return;
        }
        await command.UndoAsync();
    }

    // Returns history oldest-first for readable log output.
    public IEnumerable<string> GetHistory() =>
        _history.Reverse().Select(c => c.Description);
}
