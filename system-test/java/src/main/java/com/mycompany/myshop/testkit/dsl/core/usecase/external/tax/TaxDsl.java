package com.mycompany.myshop.testkit.dsl.core.usecase.external.tax;

import com.mycompany.myshop.testkit.common.Closer;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseContext;
import com.mycompany.myshop.testkit.dsl.core.usecase.external.tax.usecases.GetTaxRate;
import com.mycompany.myshop.testkit.dsl.core.usecase.external.tax.usecases.GoToTax;
import com.mycompany.myshop.testkit.dsl.core.usecase.external.tax.usecases.ReturnsTaxRate;
import com.mycompany.myshop.testkit.driver.port.external.tax.TaxDriver;

public class TaxDsl implements AutoCloseable {
    private final TaxDriver driver;
    private final UseCaseContext context;

    public TaxDsl(TaxDriver driver, UseCaseContext context) {
        this.driver = driver;
        this.context = context;
    }

    @Override
    public void close() {
        Closer.close(driver);
    }

    public GoToTax goToTax() {
        return new GoToTax(driver, context);
    }

    public ReturnsTaxRate returnsTaxRate() {
        return new ReturnsTaxRate(driver, context);
    }

    public GetTaxRate getTaxRate() {
        return new GetTaxRate(driver, context);
    }
}
