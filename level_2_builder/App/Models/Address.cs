namespace Builder.App.Models;

public record Address(string Street, string City, string PostCode, string Country)
{
    public override string ToString() => $"{Street}, {City} {PostCode}, {Country}";
}
