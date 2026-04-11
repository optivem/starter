package com.optivem.shop.testkit.port.assume;

import com.optivem.shop.testkit.port.assume.steps.AssumeRunning;

public interface AssumeStage {
    AssumeRunning shop();

    AssumeRunning erp();

    AssumeRunning tax();

    AssumeRunning clock();
}
