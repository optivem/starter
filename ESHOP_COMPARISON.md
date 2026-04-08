# Eshop vs Starter Comparison

Verbatim comparison of `eshop` (single Java+React app) against `starter` multitier equivalent (`system/multitier/backend-java` + `system/multitier/frontend-react`).

## Backend — Functional Divergence

| Area | eshop | starter |
|------|-------|---------|
| **Spring Boot version** | 3.3.5 | 3.5.6 |
| **OpenAPI/Swagger** | Included (springdoc dep + OpenApiConfig) | Removed |
| **H2 test DB** | Not included | Added as test dependency |
| **Health endpoint** | Uses `HealthEndpoint` actuator injection | Direct `Map.of("status", "UP")` response |
| **OrderStatus enum** | `PLACED, CANCELLED, DELIVERED` | `PLACED, CANCELLED` (no DELIVERED) |
| **Deliver endpoint** | `POST /api/orders/{orderNumber}/deliver` exists | Removed |
| **OrderService.deliverOrder()** | Exists (simple PLACED → DELIVERED transition) | Removed |
| **Promotion logic** | None | Added — calls `erpGateway.getPromotionDetails()`, applies promotion discount |
| **Cancel time window** | 22:00–23:00 | 23:59 start, different logic flow |
| **Cancel validation order** | Validates order number first, then time check | Checks year-end restriction before DB lookup |
| **CouponService constants** | Has `FIELD_VALID_FROM/TO`, `MSG_*_MUST_BE_FUTURE` | Removed (unused) |
| **Debug System.out.println** | Present in GlobalExceptionHandler, ClockGateway, OrderService | Removed (except TaxGateway) |
| **Exception types in gateways** | `RuntimeException` | `IllegalStateException` |
| **GlobalExceptionHandler** | Inline strings, debug logging | Extracted constants (`VALIDATION_DETAIL`, etc.), cleaned up |
| **TypeValidationMessageExtractor** | Public default constructor | Private constructor (utility class pattern) |
| **Repository comments** | Has explanatory comments | Comments removed |
| **Datasource config** | Single `POSTGRES_URL` env var | Structured `POSTGRES_DB_HOST/PORT/NAME/USER/PASSWORD` with defaults |
| **Server port** | Not specified | `8081` |
| **External URL defaults** | No defaults | Defaults for `ERP_API_URL`, `CLOCK_API_URL`, `TAX_API_URL`, `ALLOWED_ORIGINS` |
| **Sonar config** | Basic | Added issue ignore rules (S2699, S2638) |
| **Stream collection** | `.collect(Collectors.toList())` | `.toList()` |
| **InterruptedException** | Not handled in gateways | Handled with thread interrupt restoration |
| **Package name** | `com.optivem.eshop.backend` | `com.optivem.shop.backend` |

## Frontend — Functional Divergence

| Area | eshop | starter |
|------|-------|---------|
| **Package name** | `optivem-eshop-frontend` | `optivem-shop-frontend` |
| **Branding** | "Optivem eShop" | "Shop" |
| **Order page** | `/shop` (Shop.tsx) | `/new-order` (NewOrder.tsx) |
| **VERSION** | 1.0.0 | 1.3.0 |
| **OrderActions component** | Separate component with cancel/deliver buttons | Removed; cancel inlined in OrderDetails.tsx |
| **Deliver functionality** | Present (hook, service, UI) | Removed entirely |
| **OrderHistoryTable columns** | Has `country` and `appliedCouponCode` columns | Not displayed (data exists in types but columns not rendered) |
| **api.types.ts OrderStatus** | Has `DELIVERED` | No `DELIVERED` |
| **api.types.ts PlaceOrderRequest.country** | `country: string` (required) | `country?: string` (optional) |
| **api.types.ts appliedCouponCode** | `appliedCouponCode?: string` (optional) | `appliedCouponCode: string \| null` |
| **api.types.ts GetCouponResponse** | Exists | Missing |
| **api.types.ts inline comments** | Present on coupon fields | Removed |
| **form.types.ts couponCode** | `couponCode?: string` (optional) | `couponCode: string` (required, defaults to `""`) |
| **form.types.ts defaults** | `country: 'US'`, `couponCode: undefined` | `country: ''`, `couponCode: ''` |
| **Country validation** | Validated in form ("Country must not be empty") | Removed |

## Frontend — Code Quality Improvements in Starter

| Area | eshop | starter |
|------|-------|---------|
| **Props typing** | Direct `Props` type | `Readonly<Props>` wrapper on all components |
| **Emojis in UI** | Present (📦, 📋, 🎟️, ⚠️, 🔄) | Removed |
| **React keys** | Uses `index` as key in Breadcrumb | Uses `item.path ?? item.label` |
| **JSDoc examples** | Present in DataState, ErrorBoundary, TableDataState, useNotification, useCoupons | Removed |
| **NotificationContext** | No memoization | Context value wrapped in `useMemo` |
| **ErrorBoundary** | `window.location.href` | `globalThis.location.href` |
| **parseFloat/isNaN** | Global `parseFloat`/`isNaN` | `Number.parseFloat`/`Number.isNaN` |
| **Notification fieldErrors** | `error.fieldErrors &&` | `error.fieldErrors?.map` (optional chaining) |
| **order-service baseUrl** | `private baseUrl` | `private readonly baseUrl` |
| **Line endings** | Mixed CRLF/LF | Normalized to LF |
| **Trailing newlines** | Present in many files | Removed |
| **Comments** | More inline comments | Fewer comments |
| **Commented-out code** | Auto-dismiss notification code block | Removed |

## Delivery in eshop-tests

Delivery is extensively used in `eshop-tests`:

- **Driver port**: `ShopDriver.deliverOrder()` interface method
- **Driver adapters**: Both `ShopApiDriver` and `ShopUiDriver` implement `deliverOrder()`
- **UI page object**: `OrderDetailsPage.clickDeliverOrder()` with `[aria-label='Deliver Order']` selector
- **API client**: `OrderController.deliverOrder()` — POSTs to `/{orderNumber}/deliver`
- **DSL core**: Dedicated `DeliverOrder` use case class, exposed via `ShopDsl.deliverOrder()`
- **DTO**: `OrderStatus.DELIVERED` enum value
- **Scenario setup**: `GivenOrderImpl` uses delivery to set up orders in DELIVERED state

The delivery logic itself is minimal — just a `PLACED → DELIVERED` status transition with no business rules, time restrictions, or external service calls.

---

## Action Plan: Make Starter a Superset of Eshop

Goal: Starter should contain everything eshop has, plus its own improvements. Keep starter's newer libraries and code quality gains. Add back anything that was removed.

### Backend — Add Back from Eshop

1. **Add back delivery feature**
   - Restore `DELIVERED` to `OrderStatus` enum
   - Restore `deliverOrder()` in `OrderService` (apply starter's code quality: no debug prints, `IllegalStateException`, handle `InterruptedException` if needed)
   - Restore `POST /api/orders/{orderNumber}/deliver` endpoint in `OrderController`

2. **Add back OpenAPI/Swagger**
   - Restore `springdoc-openapi-starter-webmvc-ui` dependency in `build.gradle`
   - Restore `OpenApiConfig.java` (with starter's package name `com.optivem.shop.backend`)

3. **Align cancel logic with eshop**
   - Review the cancel time window divergence (eshop: 22:00–23:00, starter: 23:59 start) — decide which is intended and make consistent
   - Review validation order (eshop validates order number first, starter checks year-end restriction first) — pick one approach

4. **Restore CouponService constants** (`FIELD_VALID_FROM/TO`, `MSG_*_MUST_BE_FUTURE`) if they are used in eshop validation paths — if truly unused, leave removed

### Backend — Keep Starter's Improvements (no action needed)

- Spring Boot 3.5.6 (newer)
- H2 test DB dependency
- Promotion system (eshop doesn't have this — superset)
- Direct health endpoint (simpler, no actuator dependency)
- `IllegalStateException` over `RuntimeException` in gateways
- `InterruptedException` handling with thread interrupt restoration
- Extracted constants in `GlobalExceptionHandler`
- Private constructor on `TypeValidationMessageExtractor`
- Debug `System.out.println` removed (clean code)
- `.toList()` over `.collect(Collectors.toList())`
- Structured datasource config with defaults
- Sonar ignore rules

### Frontend — Add Back from Eshop

5. **Add back delivery UI**
   - Restore `OrderActions.tsx` component (with starter's code quality: `Readonly<Props>`, no emojis)
   - Restore `deliverOrder` in `order-service.ts`
   - Restore `deliverOrder` and `isDelivering` in `useOrderDetails` hook
   - Wire deliver button back into `OrderDetails.tsx` via `OrderActions`

6. **Restore OrderHistoryTable columns**
   - Add back `country` column (data already in types, just not rendered)
   - Add back `appliedCouponCode` column (data already in types, just not rendered)

7. **Restore api.types.ts missing pieces**
   - Add `DELIVERED` to `OrderStatus` enum
   - Add `GetCouponResponse` interface
   - Make `PlaceOrderRequest.country` required (not optional)

8. **Restore country validation in order form**
   - Add back country validation in `useOrderForm.ts` ("Country must not be empty")
   - Restore default `country: 'US'`

9. **Restore couponCode as optional**
   - Change `couponCode: string` back to `couponCode?: string` in `form.types.ts`
   - Restore default `couponCode: undefined`

### Frontend — Keep Starter's Improvements (no action needed)

- `Readonly<Props>` on all components
- Emojis removed from UI
- `useMemo` for context value
- `globalThis` over `window`
- `Number.parseFloat`/`Number.isNaN`
- Optional chaining for fieldErrors
- `private readonly baseUrl`
- Better React keys
- Line endings normalized to LF
- Commented-out code removed

### Decisions (resolved)

- **Cancel logic**: Keep starter's way (23:59 start, check restriction before DB lookup)
- **Route naming**: Keep starter's `/new-order`
- **Branding**: Keep "Shop"
- **CouponService constants**: Dead code (not referenced anywhere in eshop) — leave removed

---

## TODO: Migrate Delivery Test Infrastructure (separate task)

Starter's system-test has `OrderStatus.DELIVERED` in the DTO but none of the actual test infrastructure that eshop-tests has. The following would need to be migrated separately:

- `ShopDriver.deliverOrder()` — driver port interface method
- `ShopApiDriver.deliverOrder()` — API driver adapter (POSTs to `/{orderNumber}/deliver`)
- `ShopUiDriver.deliverOrder()` — UI driver adapter (Playwright)
- `OrderDetailsPage.clickDeliverOrder()` — UI page object (`[aria-label='Deliver Order']` selector)
- `OrderController.deliverOrder()` — API client method
- `DeliverOrder` use case class — DSL core
- `ShopDsl.deliverOrder()` — DSL entry point
- `GivenOrderImpl` — scenario setup uses delivery to create orders in DELIVERED state

This should be done after the app-level delivery feature is restored in starter.
