package com.mycompany.myshop.systemtest.legacy.mod11.base;

import com.mycompany.myshop.systemtest.configuration.BaseConfigurableTest;
import com.mycompany.myshop.testkit.dsl.core.usecase.UseCaseDsl;
import com.mycompany.myshop.testkit.dsl.core.ScenarioDslImpl;
import com.mycompany.myshop.testkit.dsl.port.ScenarioDsl;
import com.mycompany.myshop.testkit.common.Closer;
import com.mycompany.myshop.systemtest.infrastructure.playwright.BrowserLifecycleExtension;
import com.optivem.testing.extensions.ChannelExtension;
import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.extension.ExtendWith;

@ExtendWith({ChannelExtension.class, BrowserLifecycleExtension.class})
public class BaseScenarioDslTest extends BaseConfigurableTest {
    private UseCaseDsl app;
    protected ScenarioDsl scenario;

    @BeforeEach
    void setUp() {
        var configuration = loadConfiguration();
        app = createUseCaseDsl(configuration);
        scenario = new ScenarioDslImpl(app);
    }

    @AfterEach
    void tearDown() {
        Closer.close(app);
    }
}

