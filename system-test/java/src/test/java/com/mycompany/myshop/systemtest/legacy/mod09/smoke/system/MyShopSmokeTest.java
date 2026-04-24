package com.mycompany.myshop.systemtest.legacy.mod09.smoke.system;

import com.mycompany.myshop.systemtest.legacy.mod09.base.BaseScenarioDslTest;
import com.mycompany.myshop.testkit.channel.ChannelType;
import com.optivem.testing.Channel;
import org.junit.jupiter.api.TestTemplate;

class MyShopSmokeTest extends BaseScenarioDslTest {
    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldBeAbleToGoToMyShop() {
        scenario.assume().myShop().shouldBeRunning();
    }
}

