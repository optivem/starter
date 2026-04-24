package com.mycompany.myshop.systemtest.legacy.mod07.e2e;

import com.mycompany.myshop.testkit.channel.ChannelType;
import com.mycompany.myshop.systemtest.legacy.mod07.e2e.base.BaseE2eTest;
import com.optivem.testing.Channel;
import org.junit.jupiter.api.TestTemplate;

import static com.mycompany.myshop.systemtest.commons.constants.Defaults.*;

class PlaceOrderNegativeTest extends BaseE2eTest {
    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldRejectOrderWithNonIntegerQuantity() {
        app.myShop().placeOrder().sku(SKU).quantity("3.5").country(COUNTRY).execute()
                .shouldFail()
                .errorMessage("The request contains one or more validation errors")
                .fieldErrorMessage("quantity", "Quantity must be an integer");
    }
}
