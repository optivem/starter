package com.mycompany.myshop.systemtest.legacy.mod06.base;

import com.mycompany.myshop.systemtest.configuration.BaseConfigurableTest;
import com.mycompany.myshop.systemtest.configuration.Configuration;
import com.mycompany.myshop.testkit.driver.adapter.external.erp.ErpRealDriver;
import com.mycompany.myshop.testkit.driver.adapter.external.tax.TaxRealDriver;
import com.mycompany.myshop.testkit.channel.ChannelType;
import com.mycompany.myshop.testkit.driver.adapter.myshop.api.MyShopApiDriver;
import com.mycompany.myshop.testkit.driver.port.myshop.MyShopDriver;
import com.mycompany.myshop.testkit.driver.adapter.myshop.ui.MyShopUiDriver;
import com.mycompany.myshop.systemtest.infrastructure.playwright.BrowserLifecycleExtension;
import com.mycompany.myshop.testkit.common.Closer;
import com.optivem.testing.contexts.ChannelContext;
import com.optivem.testing.extensions.ChannelExtension;
import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.extension.ExtendWith;

@ExtendWith(ChannelExtension.class)
public class BaseChannelDriverTest extends BaseConfigurableTest {
    protected MyShopDriver myShopDriver;
    protected ErpRealDriver erpDriver;
    protected TaxRealDriver taxDriver;

    @BeforeEach
    void setUp() {
        var configuration = loadConfiguration();

        myShopDriver = createChannelMyShopDriver(configuration);
        erpDriver = new ErpRealDriver(configuration.getErpBaseUrl());
        taxDriver = new TaxRealDriver(configuration.getTaxBaseUrl());
    }

    @AfterEach
    void tearDown() {
        Closer.close(myShopDriver);
        Closer.close(erpDriver);
        Closer.close(taxDriver);
    }

    private MyShopDriver createChannelMyShopDriver(Configuration configuration) {
        var channel = ChannelContext.get();

        if(channel == null) {
            return null;
        }

        if (ChannelType.UI.equals(channel)) {
            return new MyShopUiDriver(configuration.getMyShopUiBaseUrl(), BrowserLifecycleExtension.getBrowser());
        } else if (ChannelType.API.equals(channel)) {
            return new MyShopApiDriver(configuration.getMyShopApiBaseUrl());
        } else {
            throw new IllegalStateException("Unknown channel: " + channel);
        }
    }
}
