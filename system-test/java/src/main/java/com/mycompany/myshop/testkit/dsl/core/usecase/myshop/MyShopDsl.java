package com.mycompany.myshop.testkit.dsl.core.usecase.myshop;

import com.mycompany.myshop.testkit.driver.port.myshop.MyShopDriver;
import com.mycompany.myshop.testkit.dsl.core.usecase.myshop.usecases.BrowseCoupons;
import com.mycompany.myshop.testkit.dsl.core.usecase.myshop.usecases.CancelOrder;
import com.mycompany.myshop.testkit.dsl.core.usecase.myshop.usecases.DeliverOrder;
import com.mycompany.myshop.testkit.dsl.core.usecase.myshop.usecases.GoToMyShop;
import com.mycompany.myshop.testkit.dsl.core.usecase.myshop.usecases.PlaceOrder;
import com.mycompany.myshop.testkit.dsl.core.usecase.myshop.usecases.PublishCoupon;
import com.mycompany.myshop.testkit.dsl.core.usecase.myshop.usecases.ViewOrder;
import com.mycompany.myshop.testkit.common.Closer;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseContext;

import java.io.Closeable;

public class MyShopDsl implements Closeable {
    private final MyShopDriver driver;
    private final UseCaseContext context;

    public MyShopDsl(MyShopDriver driver, UseCaseContext context) {
        this.driver = driver;
        this.context = context;
    }

    @Override
    public void close() {
        Closer.close(driver);
    }

    public GoToMyShop goToMyShop() {
        return new GoToMyShop(driver, context);
    }

    public PlaceOrder placeOrder() {
        return new PlaceOrder(driver, context);
    }

    public CancelOrder cancelOrder() {
        return new CancelOrder(driver, context);
    }

    public DeliverOrder deliverOrder() {
        return new DeliverOrder(driver, context);
    }

    public ViewOrder viewOrder() {
        return new ViewOrder(driver, context);
    }

    public PublishCoupon publishCoupon() {
        return new PublishCoupon(driver, context);
    }

    public BrowseCoupons browseCoupons() {
        return new BrowseCoupons(driver, context);
    }
}
