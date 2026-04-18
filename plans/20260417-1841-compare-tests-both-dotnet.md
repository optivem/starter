# .NET — System Test Alignment Plan

Reference report: `reports/20260417-1841-compare-tests-both.md`
Reference implementation: **Java** (align .NET to Java unless otherwise noted).

Execute the tasks below in order. Each task names the concrete target file(s) and the Java reference file to copy behavior from.

---

## 1. Architecture — Clients Layer: rename Tax DTO

Target file:
- `system-test/dotnet/Driver.Adapter/External/Tax/Client/Dtos/ExtCountryDetailsResponse.cs` → rename to `ExtGetCountryResponse.cs`.

Reference:
- Java: `system-test/java/src/main/java/com/optivem/shop/testkit/driver/adapter/external/tax/client/dtos/ExtGetCountryResponse.java`
- TypeScript: `system-test/typescript/src/testkit/driver/adapter/external/tax/client/dtos/ExtGetCountryResponse.ts`

Changes:
1. Rename the file.
2. Rename the class `ExtCountryDetailsResponse` → `ExtGetCountryResponse` inside the file.
3. Update every `using`, type reference, and `new ExtCountryDetailsResponse(...)` usage across `system-test/dotnet/` (in particular `Driver.Adapter/External/Tax/Client/TaxRealClient.cs`, `TaxStubClient.cs`, `BaseTaxClient.cs`, and anywhere else it is deserialized).

APRPOVED

## Local verification & commit

From `system-test/dotnet/`:

1. Run the latest suite:
   ```
   ./Run-SystemTests.ps1 -Architecture monolith
   ```
2. Run the legacy suite:
   ```
   ./Run-SystemTests.ps1 -Architecture monolith -Legacy
   ```
3. Fix any failures before proceeding. Do not substitute raw `dotnet test` — `Run-SystemTests.ps1` is the only supported entry point because it manages containers and configuration.
4. Commit the changes as a single logical commit:
   ```
   Align .NET testkit to Java: rename ExtGetCountryResponse, relocate SystemResults, add mod11 tax contract tests
   ```
