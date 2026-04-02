package com.optivem.shop.systemtest.latest.contract.erp;

import com.optivem.shop.systemtest.configuration.ExternalSystemMode;

class ErpStubContractTest extends BaseErpContractTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.STUB;
    }
}
