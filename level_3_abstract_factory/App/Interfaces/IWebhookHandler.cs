namespace AbstractFactory.App.Interfaces;

// Abstract Product C — verify and handle incoming provider callbacks.
// Each provider uses a different signature scheme; mixing them causes silent failures.
public interface IWebhookHandler
{
    bool   VerifySignature(string payload, string signature);
    string HandleEvent(string payload);
}
