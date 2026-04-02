package com.optivem.eshop.monolith.controllers.web;

import com.optivem.eshop.monolith.core.dtos.PlaceOrderRequest;
import com.optivem.eshop.monolith.core.dtos.PlaceOrderResponse;
import com.optivem.eshop.monolith.core.exceptions.ValidationException;
import com.optivem.eshop.monolith.core.services.OrderService;
import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.servlet.mvc.support.RedirectAttributes;

@Controller
public class ShopController {

    private final OrderService orderService;

    public ShopController(OrderService orderService) {
        this.orderService = orderService;
    }

    @GetMapping("/shop")
    public String shop() {
        return "shop";
    }

    @PostMapping("/shop")
    public String placeOrder(@RequestParam String sku,
                             @RequestParam String quantity,
                             RedirectAttributes redirectAttributes) {
        try {
            var request = new PlaceOrderRequest();
            request.setSku(sku);

            try {
                request.setQuantity(Integer.parseInt(quantity));
            } catch (NumberFormatException e) {
                redirectAttributes.addFlashAttribute("error", "Quantity must be an integer");
                redirectAttributes.addFlashAttribute("sku", sku);
                redirectAttributes.addFlashAttribute("quantity", quantity);
                return "redirect:/shop";
            }

            if (request.getQuantity() <= 0) {
                redirectAttributes.addFlashAttribute("error", "Quantity must be positive");
                redirectAttributes.addFlashAttribute("sku", sku);
                redirectAttributes.addFlashAttribute("quantity", quantity);
                return "redirect:/shop";
            }

            if (request.getSku() == null || request.getSku().isBlank()) {
                redirectAttributes.addFlashAttribute("error", "SKU must not be empty");
                redirectAttributes.addFlashAttribute("sku", sku);
                redirectAttributes.addFlashAttribute("quantity", quantity);
                return "redirect:/shop";
            }

            PlaceOrderResponse response = orderService.placeOrder(request);
            redirectAttributes.addFlashAttribute("success",
                    "Success! Order has been created with Order Number " + response.getOrderNumber());
            return "redirect:/shop";
        } catch (ValidationException e) {
            redirectAttributes.addFlashAttribute("error", e.getMessage());
            redirectAttributes.addFlashAttribute("sku", sku);
            redirectAttributes.addFlashAttribute("quantity", quantity);
            return "redirect:/shop";
        } catch (Exception e) {
            redirectAttributes.addFlashAttribute("error", "An unexpected error occurred: " + e.getMessage());
            redirectAttributes.addFlashAttribute("sku", sku);
            redirectAttributes.addFlashAttribute("quantity", quantity);
            return "redirect:/shop";
        }
    }
}
