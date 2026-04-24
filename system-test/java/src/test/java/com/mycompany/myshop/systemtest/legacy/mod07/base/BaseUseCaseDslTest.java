package com.mycompany.myshop.systemtest.legacy.mod07.base;

import com.mycompany.myshop.systemtest.configuration.BaseConfigurableTest;
import com.mycompany.myshop.testkit.dsl.core.usecase.UseCaseDsl;
import com.mycompany.myshop.testkit.common.Closer;
import com.optivem.testing.extensions.ChannelExtension;
import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.extension.ExtendWith;

@ExtendWith(ChannelExtension.class)
public class BaseUseCaseDslTest extends BaseConfigurableTest {
    protected UseCaseDsl app;

    @BeforeEach
    void setUp() {
        var configuration = loadConfiguration();
        app = createUseCaseDsl(configuration);
    }

    @AfterEach
    void tearDown() {
        Closer.close(app);
    }
}

