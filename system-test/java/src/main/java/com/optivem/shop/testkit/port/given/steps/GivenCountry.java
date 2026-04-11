package com.optivem.shop.testkit.port.given.steps;

import com.optivem.shop.testkit.port.given.steps.base.GivenStep;

public interface GivenCountry extends GivenStep {
    GivenCountry withCode(String country);

    GivenCountry withTaxRate(String taxRate);

    GivenCountry withTaxRate(double taxRate);
}
