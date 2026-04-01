package com.optivem.eshop.systemtest.latest.contract.clock;

import com.optivem.eshop.systemtest.configuration.ExternalSystemMode;

class ClockRealContractTest extends BaseClockContractTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.REAL;
    }
}
