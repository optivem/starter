package com.optivem.shop.systemtest.legacy.mod08.e2e.base;

import com.optivem.shop.systemtest.legacy.mod08.base.BaseScenarioDslTest;
import com.optivem.shop.systemtest.configuration.ExternalSystemMode;

public class BaseE2eTest extends BaseScenarioDslTest {
    
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.REAL;
    }
}




