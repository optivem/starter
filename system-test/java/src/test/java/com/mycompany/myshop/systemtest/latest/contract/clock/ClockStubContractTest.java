package com.mycompany.myshop.systemtest.latest.contract.clock;

import com.mycompany.myshop.systemtest.configuration.ExternalSystemMode;

class ClockStubContractTest extends BaseClockContractTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.STUB;
    }
}
