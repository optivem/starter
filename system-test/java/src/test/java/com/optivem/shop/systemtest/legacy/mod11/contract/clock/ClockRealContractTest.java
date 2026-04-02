package com.optivem.shop.systemtest.legacy.mod11.contract.clock;

import com.optivem.shop.systemtest.configuration.ExternalSystemMode;

class ClockRealContractTest extends BaseClockContractTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.REAL;
    }
}



