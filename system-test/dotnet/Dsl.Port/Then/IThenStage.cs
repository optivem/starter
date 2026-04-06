using Dsl.Port.Then.Steps;

namespace Dsl.Port.Then;

public interface IThenStage
{
    Task<IThenClock> Clock();

    Task<IThenProduct> Product(string skuAlias);

    Task<IThenCountry> Country(string countryAlias);
}
