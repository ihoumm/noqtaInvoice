using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Domain.Tests.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Create_RoundsAmountToTwoDecimalPlaces()
    {
        var money = Money.Create(10.126m, Currency.MAD);

        Assert.Equal(10.13m, money.Amount);
        Assert.Equal(Currency.MAD, money.Currency);
    }

    [Fact]
    public void Add_WithSameCurrency_ReturnsCombinedAmount()
    {
        var first = Money.Create(100m, Currency.MAD);
        var second = Money.Create(50m, Currency.MAD);

        var result = first.Add(second);

        Assert.Equal(150m, result.Amount);
        Assert.Equal(Currency.MAD, result.Currency);
    }

    [Fact]
    public void Add_WithDifferentCurrencies_Throws()
    {
        var first = Money.Create(100m, Currency.MAD);
        var second = Money.Create(50m, Currency.Create("EUR"));

        Assert.Throws<InvalidOperationException>(
            () => first.Add(second));
    }
}