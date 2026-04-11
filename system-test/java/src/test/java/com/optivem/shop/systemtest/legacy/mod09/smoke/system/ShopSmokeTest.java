package com.optivem.shop.systemtest.legacy.mod09.smoke.system;

import com.optivem.shop.systemtest.legacy.mod09.base.BaseScenarioDslTest;
import com.optivem.shop.testkit.channel.ChannelType;
import com.optivem.testing.Channel;
import org.junit.jupiter.api.TestTemplate;

class ShopSmokeTest extends BaseScenarioDslTest {
    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldBeAbleToGoToShop() {
        scenario.assume().shop().shouldBeRunning();
    }
}

