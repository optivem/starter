using Dsl.Port.Given.Steps.Base;

namespace Dsl.Port.Given.Steps;

public interface IGivenCountry : IGivenStep
{
    IGivenCountry WithCode(string country);

    IGivenCountry WithTaxRate(string taxRate);

    IGivenCountry WithTaxRate(decimal taxRate);
}
