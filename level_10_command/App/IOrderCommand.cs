namespace Command.App;

// PATTERN CONCEPT: Command interface.
//
// Every command must be able to Execute AND Undo itself.
// The invoker calls these methods — it never knows what they do internally.
// The Description is used for the history log.
public interface IOrderCommand
{
    Task   ExecuteAsync();
    Task   UndoAsync();
    string Description { get; }
}
