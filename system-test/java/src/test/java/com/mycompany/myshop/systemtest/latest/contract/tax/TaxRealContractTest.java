package com.mycompany.myshop.systemtest.latest.contract.tax;

import com.mycompany.myshop.systemtest.configuration.ExternalSystemMode;

public class TaxRealContractTest extends BaseTaxContractTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.REAL;
    }
}
