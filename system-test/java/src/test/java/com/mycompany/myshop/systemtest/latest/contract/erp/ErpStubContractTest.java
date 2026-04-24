package com.mycompany.myshop.systemtest.latest.contract.erp;

import com.mycompany.myshop.systemtest.configuration.ExternalSystemMode;

class ErpStubContractTest extends BaseErpContractTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.STUB;
    }
}
