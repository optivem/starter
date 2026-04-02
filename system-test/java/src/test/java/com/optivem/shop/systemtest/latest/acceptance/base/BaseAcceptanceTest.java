package com.optivem.shop.systemtest.latest.acceptance.base;

import com.optivem.shop.systemtest.latest.base.BaseScenarioDslTest;
import com.optivem.shop.systemtest.configuration.ExternalSystemMode;

public class BaseAcceptanceTest extends BaseScenarioDslTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.STUB;
    }
}


