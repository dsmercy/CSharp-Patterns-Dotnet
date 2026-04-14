using Mediator.App.Models;

namespace Mediator.App;

// PATTERN CONCEPT: Mediator interface.
//
// This is the ONLY thing colleagues know about. They call NotifyAsync to signal
// that something happened — they have no knowledge of what comes next.
// The concrete mediator decides what to do with each event.
public interface ICheckoutMediator
{
    Task NotifyAsync(CheckoutColleague sender, CheckoutEvent checkoutEvent, Order order);
}
