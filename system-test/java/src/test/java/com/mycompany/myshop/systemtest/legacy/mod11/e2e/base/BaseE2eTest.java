package com.mycompany.myshop.systemtest.legacy.mod11.e2e.base;

import com.mycompany.myshop.systemtest.legacy.mod11.base.BaseScenarioDslTest;
import com.mycompany.myshop.systemtest.configuration.ExternalSystemMode;

public class BaseE2eTest extends BaseScenarioDslTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.REAL;
    }
}




