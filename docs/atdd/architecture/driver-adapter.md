# Driver Adapter Rules

## Real vs Stub Implementations

Each external system driver has two implementations:

- **`XyzRealDriver`** — connects to the real service via HTTP.
- **`XyzStubDriver`** — configures WireMock stubs to simulate the service.

Both implementations share a `BaseXyzDriver` that delegates to a `BaseXyzClient`. The client is also split:

- **`XyzRealClient`** — extends `BaseXyzClient` with methods that make real HTTP calls (e.g. `createProduct()`).
- **`XyzStubClient`** — extends `BaseXyzClient` with methods that register WireMock stubs (e.g. `configureGetProduct()`).

## External DTOs

All DTOs used by driver adapters to communicate with external systems use an `Ext*` prefix (e.g. `ExtCreateProductRequest`, `ExtProductDetailsResponse`, `ExtErpErrorResponse`).

- `Ext*Request` DTOs must use only string fields — never numeric, boolean, or other non-string types. This allows invalid values to pass through for negative test scenarios. Type conversion happens inside the HTTP client or serialization layer. See `language-equivalents.md` for the string field type and DTO boilerplate per language.
- `Ext*Response` DTOs may use typed fields (e.g. `BigDecimal`, `decimal`, `Decimal`) since they are only used for deserialization, not for constructing negative test inputs.

## goTo*() Methods

`goTo*()` methods (e.g. `goToShop()`, `goToErp()`) are health checks that verify the system is accessible. They must be called before any other driver methods in the Assume stage.

## Shop API Driver

The shop API driver uses a controller-per-resource pattern. `ShopApiClient` composes multiple controllers (e.g. `OrderController`, `CouponController`, `ProductController`), each managing one API endpoint group.

Endpoint URLs are encoded as **constants** inside each resource controller. When a system route is renamed, update the constant — that is the one place in the test layer that needs to change. The driver port interface stays untouched, so existing acceptance and contract tests keep compiling.

Not every system endpoint has a method on the API driver. Some operations are exercised through the UI driver only, so a rename of those endpoints is absorbed entirely by the system-side fetch URLs and never reaches the API adapter. Check both drivers when scoping a system-side change.

Error responses from the shop API (e.g. `ProblemDetailResponse`) are mapped to the domain `ErrorResponse` via a `SystemErrorMapper`. Never expose API-specific error formats beyond the adapter layer.

## Shop UI Driver

- UI drivers must never navigate directly to a URL. Always simulate real user behaviour by starting from the home page and clicking through the UI.
- Page objects use `aria-label` selectors for inputs and interactive elements.
- Page objects read operation results from notification elements (`[role='alert']` with `.notification.success` or `.notification.error` classes).
- The UI driver manages an internal page state enum to avoid redundant navigation.
