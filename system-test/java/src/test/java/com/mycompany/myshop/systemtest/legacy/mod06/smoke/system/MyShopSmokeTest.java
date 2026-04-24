package com.mycompany.myshop.systemtest.legacy.mod06.smoke.system;

import com.mycompany.myshop.systemtest.legacy.mod06.base.BaseChannelDriverTest;
import com.mycompany.myshop.testkit.channel.ChannelType;
import com.optivem.testing.Channel;
import org.junit.jupiter.api.TestTemplate;

import static com.mycompany.myshop.testkit.common.ResultAssert.assertThatResult;

class MyShopSmokeTest extends BaseChannelDriverTest {
    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldBeAbleToGoToMyShop() {
        var result = myShopDriver.goToMyShop();
        assertThatResult(result).isSuccess();
    }
}

