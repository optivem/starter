package com.optivem.shop.dsl.core.scenario.given;

import com.optivem.shop.dsl.core.usecase.UseCaseDsl;
import com.optivem.shop.dsl.core.scenario.then.ThenImpl;
import com.optivem.shop.dsl.core.scenario.given.steps.GivenClockImpl;
import com.optivem.shop.dsl.core.scenario.given.steps.GivenOrderImpl;
import com.optivem.shop.dsl.core.scenario.given.steps.GivenProductImpl;
import com.optivem.shop.dsl.core.scenario.given.steps.GivenPromotionImpl;
import com.optivem.shop.dsl.port.given.GivenStage;
import com.optivem.shop.dsl.port.then.ThenStage;
import com.optivem.shop.dsl.core.scenario.when.WhenImpl;

import java.util.ArrayList;
import java.util.List;

public class GivenImpl implements GivenStage {
    private final UseCaseDsl app;
    private GivenClockImpl clock;
    private GivenPromotionImpl promotion;
    private final List<GivenProductImpl> products;
    private final List<GivenOrderImpl> orders;

    public GivenImpl(UseCaseDsl app) {
        this.app = app;
        this.clock = null;
        this.promotion = new GivenPromotionImpl(this);
        this.products = new ArrayList<>();
        this.orders = new ArrayList<>();
    }

    public GivenProductImpl product() {
        var product = new GivenProductImpl(this);
        products.add(product);
        return product;
    }

    public GivenOrderImpl order() {
        var order = new GivenOrderImpl(this);
        orders.add(order);
        return order;
    }

    public GivenClockImpl clock() {
        clock = new GivenClockImpl(this);
        return clock;
    }

    @Override
    public GivenPromotionImpl promotion() {
        promotion = new GivenPromotionImpl(this);
        return promotion;
    }

    public WhenImpl when() {
        setup();
        return new WhenImpl(app, !products.isEmpty(), true);
    }

    public ThenStage then() {
        setup();
        return new ThenImpl(app);
    }

    private void setup() {
        setupClock();
        setupPromotion();
        setupErp();
        setupShop();
    }

    private void setupPromotion() {
        promotion.execute(app);
    }

    private void setupClock() {
        if(clock != null) {
            clock.execute(app);
        }
    }

    private void setupErp() {
        if (!orders.isEmpty() && products.isEmpty()) {
            var defaultProduct = new GivenProductImpl(this);
            products.add(defaultProduct);
        }

        for (var product : products) {
            product.execute(app);
        }
    }

    private void setupShop() {
        for (var order : orders) {
            order.execute(app);
        }
    }
}
