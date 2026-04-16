namespace Dsl.Port.Then.Steps;

public interface IThenProduct
{
    IThenProduct HasSku(string sku);

    IThenProduct HasPrice(decimal price);

    Task<IThenClock> Clock();

    Task<IThenProduct> Product(string skuAlias);

    Task<IThenCountry> Country(string countryAlias);
}
