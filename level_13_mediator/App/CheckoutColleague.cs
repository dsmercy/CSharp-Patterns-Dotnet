namespace Mediator.App;

// PATTERN CONCEPT: Abstract Colleague.
//
// Every component in the checkout workflow inherits from this.
// It stores only one reference — the mediator — and nothing else.
// Colleagues never hold references to each other.
//
// Real-world parallel: aircraft in air traffic control. Each aircraft (colleague)
// communicates only with the control tower (mediator). Aircraft never coordinate
// with each other directly.
public abstract class CheckoutColleague(ICheckoutMediator mediator)
{
    protected readonly ICheckoutMediator _mediator = mediator;
}
