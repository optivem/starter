package com.mycompany.myshop.testkit.driver.adapter.myshop.ui.client.pages;

import com.mycompany.myshop.testkit.driver.adapter.shared.client.playwright.PageClient;

public class HomePage extends BasePage {
    private static final String MY_SHOP_BUTTON_SELECTOR = "a[href='/new-order']";
    private static final String ORDER_HISTORY_BUTTON_SELECTOR = "a[href='/order-history']";
    private static final String COUPON_MANAGEMENT_BUTTON_SELECTOR = "a[href='/admin-coupons']";

    public HomePage(PageClient pageClient) {
        super(pageClient);
    }

    public NewOrderPage clickNewOrder() {
        pageClient.click(MY_SHOP_BUTTON_SELECTOR);
        return new NewOrderPage(pageClient);
    }

    public OrderHistoryPage clickOrderHistory() {
        pageClient.click(ORDER_HISTORY_BUTTON_SELECTOR);
        return new OrderHistoryPage(pageClient);
    }

    public CouponManagementPage clickCouponManagement() {
        pageClient.click(COUPON_MANAGEMENT_BUTTON_SELECTOR);
        return new CouponManagementPage(pageClient);
    }
}
