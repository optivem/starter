package com.mycompany.myshop.testkit.dsl.core.usecase.external.tax.usecases;

import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseContext;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseResult;
import com.mycompany.myshop.testkit.dsl.core.shared.VoidVerification;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.error.SystemError;
import com.mycompany.myshop.testkit.dsl.core.usecase.external.tax.usecases.base.BaseTaxUseCase;
import com.mycompany.myshop.testkit.driver.port.external.tax.TaxDriver;

public class GoToTax extends BaseTaxUseCase<Void, VoidVerification> {
    public GoToTax(TaxDriver driver, UseCaseContext context) {
        super(driver, context);
    }

    @Override
    public UseCaseResult<Void, VoidVerification> execute() {
        var result = driver.goToTax();
        return new UseCaseResult<>(result.mapError(e -> SystemError.of(e.getMessage())), context, VoidVerification::new);
    }
}
