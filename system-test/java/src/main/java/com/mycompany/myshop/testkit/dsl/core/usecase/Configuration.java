package com.mycompany.myshop.testkit.dsl.core.usecase;

import com.mycompany.myshop.testkit.dsl.port.ExternalSystemMode;
import lombok.AllArgsConstructor;
import lombok.Getter;

@Getter
@AllArgsConstructor
public class Configuration {
    private final String myShopUiBaseUrl;
    private final String myShopApiBaseUrl;
    private final String erpBaseUrl;
    private final String clockBaseUrl;
    private final ExternalSystemMode externalSystemMode;
}
