namespace Dsl.Port.Then.Steps;

public interface IThenClock
{
    IThenClock HasTime(string time);

    IThenClock HasTime();

    Task<IThenClock> Clock();

    Task<IThenProduct> Product(string skuAlias);

    Task<IThenCountry> Country(string countryAlias);
}
