package com.optivem.shop.systemtest.legacy.mod06.e2e.base;

import com.optivem.shop.systemtest.legacy.mod06.base.BaseChannelDriverTest;
import com.optivem.shop.systemtest.configuration.ExternalSystemMode;

public abstract class BaseE2eTest extends BaseChannelDriverTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.REAL;
    }
}



