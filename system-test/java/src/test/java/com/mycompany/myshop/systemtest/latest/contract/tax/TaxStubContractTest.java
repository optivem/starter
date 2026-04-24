package com.mycompany.myshop.systemtest.latest.contract.tax;

import com.mycompany.myshop.systemtest.configuration.ExternalSystemMode;
import org.junit.jupiter.api.Test;

class TaxStubContractTest extends BaseTaxContractTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.STUB;
    }

    @Test
    void shouldBeAbleToGetConfiguredTaxRate() {
        scenario
                .given().country().withCode("LALA").withTaxRate(0.23)
                .then().country("LALA").hasCountry("LALA").hasTaxRate(0.23);
    }
}
