package com.mycompany.myshop.systemtest.latest.contract.erp;

import com.mycompany.myshop.systemtest.configuration.ExternalSystemMode;

class ErpRealContractTest extends BaseErpContractTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.REAL;
    }
}
