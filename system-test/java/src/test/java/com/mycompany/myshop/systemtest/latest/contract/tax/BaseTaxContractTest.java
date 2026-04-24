package com.mycompany.myshop.systemtest.latest.contract.tax;

import com.mycompany.myshop.systemtest.latest.contract.base.BaseExternalSystemContractTest;
import org.junit.jupiter.api.Test;

public abstract class BaseTaxContractTest extends BaseExternalSystemContractTest {
    @Test
    void shouldBeAbleToGetTaxRate() {
        scenario
                .given().country().withCode("US").withTaxRate(0.09)
                .then().country("US").hasTaxRateIsPositive();
    }
}
