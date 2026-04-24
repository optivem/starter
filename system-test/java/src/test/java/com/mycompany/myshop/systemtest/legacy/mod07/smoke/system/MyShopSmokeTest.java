package com.mycompany.myshop.systemtest.legacy.mod07.smoke.system;

import com.mycompany.myshop.systemtest.legacy.mod07.base.BaseUseCaseDslTest;
import com.mycompany.myshop.testkit.channel.ChannelType;
import com.optivem.testing.Channel;
import org.junit.jupiter.api.TestTemplate;

class MyShopSmokeTest extends BaseUseCaseDslTest {
    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldBeAbleToGoToMyShop() {
        app.myShop().goToMyShop()
                .execute()
                .shouldSucceed();
    }
}

