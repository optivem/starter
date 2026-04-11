package com.optivem.shop.systemtest.latest.smoke.system;

import com.optivem.shop.systemtest.latest.base.BaseScenarioDslTest;
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

