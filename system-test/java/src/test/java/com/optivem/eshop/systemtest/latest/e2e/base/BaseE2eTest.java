package com.optivem.eshop.systemtest.latest.e2e.base;

import com.optivem.eshop.systemtest.latest.base.BaseScenarioDslTest;
import com.optivem.eshop.systemtest.configuration.ExternalSystemMode;

public class BaseE2eTest extends BaseScenarioDslTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.REAL;
    }
}




