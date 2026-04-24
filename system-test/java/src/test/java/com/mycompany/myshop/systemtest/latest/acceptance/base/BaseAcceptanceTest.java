package com.mycompany.myshop.systemtest.latest.acceptance.base;

import com.mycompany.myshop.systemtest.latest.base.BaseScenarioDslTest;
import com.mycompany.myshop.systemtest.configuration.ExternalSystemMode;

public class BaseAcceptanceTest extends BaseScenarioDslTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.STUB;
    }
}


