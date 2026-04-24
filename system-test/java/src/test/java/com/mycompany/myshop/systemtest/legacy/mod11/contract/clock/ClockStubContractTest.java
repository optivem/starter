package com.mycompany.myshop.systemtest.legacy.mod11.contract.clock;

import com.mycompany.myshop.systemtest.configuration.ExternalSystemMode;

class ClockStubContractTest extends BaseClockContractTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.STUB;
    }
}
