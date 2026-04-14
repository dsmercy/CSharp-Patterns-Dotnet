namespace Facade.App.Models;

public record Address(string Street, string City, string PostCode, string Country = "UK")
{
    public override string ToString() => $"{Street}, {City} {PostCode}";
}
