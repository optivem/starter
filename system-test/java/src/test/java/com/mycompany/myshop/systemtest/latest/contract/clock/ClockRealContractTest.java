package com.mycompany.myshop.systemtest.latest.contract.clock;

import com.mycompany.myshop.systemtest.configuration.ExternalSystemMode;

class ClockRealContractTest extends BaseClockContractTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.REAL;
    }
}
