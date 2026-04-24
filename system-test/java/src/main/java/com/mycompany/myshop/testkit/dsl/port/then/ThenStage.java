package com.mycompany.myshop.testkit.dsl.port.then;

import com.mycompany.myshop.testkit.dsl.port.then.steps.ThenClock;
import com.mycompany.myshop.testkit.dsl.port.then.steps.ThenCountry;
import com.mycompany.myshop.testkit.dsl.port.then.steps.ThenProduct;

public interface ThenStage {
    ThenClock clock();

    ThenProduct product(String skuAlias);

    ThenCountry country(String countryAlias);
}
