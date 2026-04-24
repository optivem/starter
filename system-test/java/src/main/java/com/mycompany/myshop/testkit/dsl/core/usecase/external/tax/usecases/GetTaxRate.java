package com.mycompany.myshop.testkit.dsl.core.usecase.external.tax.usecases;

import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseContext;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseResult;
import com.mycompany.myshop.testkit.dsl.core.usecase.external.tax.usecases.base.BaseTaxUseCase;
import com.mycompany.myshop.testkit.driver.port.external.tax.TaxDriver;
import com.mycompany.myshop.testkit.driver.port.external.tax.dtos.GetTaxResponse;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.error.SystemError;

public class GetTaxRate extends BaseTaxUseCase<GetTaxResponse, GetTaxVerification> {
    private String countryValueOrAlias;

    public GetTaxRate(TaxDriver driver, UseCaseContext context) {
        super(driver, context);
    }

    public GetTaxRate country(String countryValueOrAlias) {
        this.countryValueOrAlias = countryValueOrAlias;
        return this;
    }

    @Override
    public UseCaseResult<GetTaxResponse, GetTaxVerification> execute() {
        var country = context.getParamValueOrLiteral(countryValueOrAlias);

        var result = driver.getTaxRate(country);

        return new UseCaseResult<>(result.mapError(e -> SystemError.of(e.getMessage())), context, GetTaxVerification::new);
    }
}
