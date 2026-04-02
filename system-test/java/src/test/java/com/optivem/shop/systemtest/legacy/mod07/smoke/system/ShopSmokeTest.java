package com.optivem.shop.systemtest.legacy.mod07.smoke.system;

import com.optivem.shop.systemtest.legacy.mod07.base.BaseUseCaseDslTest;
import com.optivem.shop.dsl.channel.ChannelType;
import com.optivem.testing.Channel;
import org.junit.jupiter.api.TestTemplate;

class ShopSmokeTest extends BaseUseCaseDslTest {
    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldBeAbleToGoToShop() {
        app.shop().goToShop()
                .execute()
                .shouldSucceed();
    }
}

