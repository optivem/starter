namespace Dsl.Port.Then.Steps;

public interface IThenCountry
{
    IThenCountry HasCountry(string country);

    IThenCountry HasTaxRate(decimal taxRate);

    IThenCountry HasTaxRateIsPositive();

    Task<IThenClock> Clock();

    Task<IThenProduct> Product(string skuAlias);

    Task<IThenCountry> Country(string countryAlias);
}
