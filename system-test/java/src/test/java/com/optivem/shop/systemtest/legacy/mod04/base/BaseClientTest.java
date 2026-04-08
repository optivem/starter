package com.optivem.shop.systemtest.legacy.mod04.base;

import com.optivem.shop.systemtest.configuration.BaseConfigurableTest;
import com.optivem.shop.systemtest.configuration.Configuration;
import com.optivem.shop.dsl.driver.adapter.external.erp.client.ErpRealClient;
import com.optivem.shop.dsl.driver.adapter.external.tax.client.TaxRealClient;
import com.optivem.shop.dsl.driver.adapter.shop.api.client.ShopApiClient;
import com.optivem.shop.dsl.driver.adapter.shop.ui.client.ShopUiClient;
import com.optivem.shop.systemtest.infrastructure.playwright.BrowserLifecycleExtension;
import com.optivem.shop.dsl.common.Closer;
import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.BeforeEach;

public class BaseClientTest extends BaseConfigurableTest {
    protected Configuration configuration;

    protected ShopUiClient shopUiClient;
    protected ShopApiClient shopApiClient;
    protected ErpRealClient erpClient;
    protected TaxRealClient taxClient;

    @BeforeEach
    protected void setUpConfiguration() {
        configuration = loadConfiguration();
    }

    protected void setUpShopUiClient() {
        shopUiClient = new ShopUiClient(configuration.getShopUiBaseUrl(), BrowserLifecycleExtension.getBrowser());
    }

    protected void setUpShopApiClient() {
        shopApiClient = new ShopApiClient(configuration.getShopApiBaseUrl());
    }

    protected void setUpExternalClients() {
        erpClient = new ErpRealClient(configuration.getErpBaseUrl());
        taxClient = new TaxRealClient(configuration.getTaxBaseUrl());
    }

    @AfterEach
    void tearDown() {
        Closer.close(shopUiClient);
        Closer.close(shopApiClient);
        Closer.close(erpClient);
        Closer.close(taxClient);
    }

}
