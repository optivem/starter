package com.optivem.eshop.systemtest.latest.contract.erp;

import com.optivem.eshop.systemtest.configuration.ExternalSystemMode;

class ErpRealContractTest extends BaseErpContractTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.REAL;
    }
}
