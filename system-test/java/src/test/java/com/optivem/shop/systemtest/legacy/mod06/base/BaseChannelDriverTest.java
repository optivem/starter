package com.optivem.shop.systemtest.legacy.mod06.base;

import com.optivem.shop.systemtest.configuration.BaseConfigurableTest;
import com.optivem.shop.systemtest.configuration.Configuration;
import com.optivem.shop.dsl.driver.adapter.external.erp.ErpRealDriver;
import com.optivem.shop.dsl.driver.adapter.external.tax.TaxRealDriver;
import com.optivem.shop.dsl.channel.ChannelType;
import com.optivem.shop.dsl.driver.adapter.shop.api.ShopApiDriver;
import com.optivem.shop.dsl.driver.port.shop.ShopDriver;
import com.optivem.shop.dsl.driver.adapter.shop.ui.ShopUiDriver;
import com.optivem.shop.systemtest.infrastructure.playwright.BrowserLifecycleExtension;
import com.optivem.shop.dsl.common.Closer;
import com.optivem.testing.contexts.ChannelContext;
import com.optivem.testing.extensions.ChannelExtension;
import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.extension.ExtendWith;

@ExtendWith(ChannelExtension.class)
public class BaseChannelDriverTest extends BaseConfigurableTest {
    protected ShopDriver shopDriver;
    protected ErpRealDriver erpDriver;
    protected TaxRealDriver taxDriver;

    @BeforeEach
    void setUp() {
        var configuration = loadConfiguration();

        shopDriver = createChannelShopDriver(configuration);
        erpDriver = new ErpRealDriver(configuration.getErpBaseUrl());
        taxDriver = new TaxRealDriver(configuration.getTaxBaseUrl());
    }

    @AfterEach
    void tearDown() {
        Closer.close(shopDriver);
        Closer.close(erpDriver);
        Closer.close(taxDriver);
    }

    private ShopDriver createChannelShopDriver(Configuration configuration) {
        var channel = ChannelContext.get();

        if(channel == null) {
            return null;
        }

        if (ChannelType.UI.equals(channel)) {
            return new ShopUiDriver(configuration.getShopUiBaseUrl(), BrowserLifecycleExtension.getBrowser());
        } else if (ChannelType.API.equals(channel)) {
            return new ShopApiDriver(configuration.getShopApiBaseUrl());
        } else {
            throw new IllegalStateException("Unknown channel: " + channel);
        }
    }
}
