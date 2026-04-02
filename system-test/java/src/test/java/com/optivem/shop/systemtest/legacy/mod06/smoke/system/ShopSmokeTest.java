package com.optivem.shop.systemtest.legacy.mod06.smoke.system;

import com.optivem.shop.systemtest.legacy.mod06.base.BaseChannelDriverTest;
import com.optivem.shop.dsl.channel.ChannelType;
import com.optivem.testing.Channel;
import org.junit.jupiter.api.TestTemplate;

import static com.optivem.shop.dsl.common.ResultAssert.assertThatResult;

class ShopSmokeTest extends BaseChannelDriverTest {
    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldBeAbleToGoToShop() {
        var result = shopDriver.goToShop();
        assertThatResult(result).isSuccess();
    }
}

