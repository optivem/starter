# Testkit Architecture Rules

Cross-language rules that apply equally to Java, .NET, and TypeScript in the `system-test/` testkit. Unlike the language-specific exceptions in `test-comparator.md`, these rules are universal — a violation in any language is always a mismatch to flag.

Referenced by `.claude/agents/test-comparator.md`.

---

## DTO Naming: `Ext` Prefix is Layer-Based

External-system DTOs (clock, ERP, tax) exist at two architectural layers. Whether the type name uses the `Ext` prefix is determined by **layer**, not by language.

- **Adapter layer** (`driver/adapter/external/<system>/client/dtos/`): DTOs **use the `Ext` prefix**.
  - Examples: `ExtGetTimeResponse`, `ExtClockErrorResponse`, `ExtGetPromotionResponse`, `ExtCreateProductRequest`, `ExtProductDetailsResponse`, `ExtErpErrorResponse`, `ExtGetCountryResponse`, `ExtTaxErrorResponse`.
  - Rationale: these mirror the external service's wire contract. The `Ext` prefix marks "wire shape" and prevents collision with the port-level type of the same conceptual name.
- **Port layer** (`driver/port/external/<system>/dtos/`): DTOs **do NOT use the `Ext` prefix**.
  - Examples: `GetTimeResponse`, `ClockErrorResponse`, `GetPromotionResponse`, `ErpErrorResponse`, `GetCountryRequest`, `GetTaxResponse`, `TaxErrorResponse`.
  - Rationale: port DTOs are the abstraction the test code consumes. The port namespace/path already disambiguates; a prefix would be noise.

When comparing across languages, flag as mismatches:
- A port-layer DTO missing in one language while present in others (e.g. TS lacks `GetPromotionResponse.ts` while Java/.NET both have it).
- A port-layer DTO using the `Ext` prefix (violates the rule — the prefix belongs only on adapter DTOs).
- An adapter-layer DTO without the `Ext` prefix (violates the rule — adapter DTOs must be prefixed).
