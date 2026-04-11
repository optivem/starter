package com.optivem.shop.testkit.port.given.steps;

import com.optivem.shop.testkit.port.given.steps.base.GivenStep;

public interface GivenClock extends GivenStep {
    GivenClock withTime();
    GivenClock withTime(String time);
    GivenClock withWeekday();
    GivenClock withWeekend();
}

