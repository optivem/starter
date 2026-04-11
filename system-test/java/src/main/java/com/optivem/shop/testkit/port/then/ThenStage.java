package com.optivem.shop.testkit.port.then;

import com.optivem.shop.testkit.port.then.steps.ThenClock;
import com.optivem.shop.testkit.port.then.steps.ThenCountry;
import com.optivem.shop.testkit.port.then.steps.ThenProduct;

public interface ThenStage {
    ThenClock clock();

    ThenProduct product(String skuAlias);

    ThenCountry country(String countryAlias);
}
