package com.optivem.shop.systemtest.legacy.mod07.e2e.base;

import com.optivem.shop.systemtest.legacy.mod07.base.BaseUseCaseDslTest;
import com.optivem.shop.systemtest.configuration.ExternalSystemMode;

public abstract class BaseE2eTest extends BaseUseCaseDslTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.REAL;
    }
}




