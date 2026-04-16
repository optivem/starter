# System Test Configuration (Latest)
# This file contains latest suite configuration values for Run-SystemTests.ps1

$Config = @{

    Suites = @(

        # === Smoke Tests (stub) ===
        @{  Id = "smoke-stub";
            SampleTest = "shouldBeAbleToGoToClock";
            Name = "latest - Smoke (stub)";
            Command = "& .\gradlew.bat test -Dversion=latest -Dtype=smoke -DexternalSystemMode=stub -Denvironment=local";
            Path = ".";
            TestReportPath = "build\reports\tests\test\index.html";
            TestInstallCommands = $null; },

        # === Smoke Tests (real) ===
        @{  Id = "smoke-real";
            SampleTest = "shouldBeAbleToGoToClock";
            Name = "latest - Smoke (real)";
            Command = "& .\gradlew.bat test -Dversion=latest -Dtype=smoke -DexternalSystemMode=real -Denvironment=local";
            Path = ".";
            TestReportPath = "build\reports\tests\test\index.html";
            TestInstallCommands = $null; },

        # === Acceptance Tests (stub) - API ===
        @{  Id = "acceptance-api";
            SampleTest = "shouldBeAbleToPlaceOrderForValidInput";
            Name = "latest - Acceptance (stub) - API";
            Command = "& .\gradlew.bat test -Dversion=latest -Dtype=acceptance -DexcludeTags=isolated -Dchannel=API -Denvironment=local";
            Path = ".";
            TestReportPath = "build\reports\tests\test\index.html";
            TestInstallCommands = $null; },

        # === Acceptance Tests (stub) - UI ===
        @{  Id = "acceptance-ui";
            SampleTest = "shouldBeAbleToPlaceOrderForValidInput";
            Name = "latest - Acceptance (stub) - UI";
            Command = "& .\gradlew.bat test -Dversion=latest -Dtype=acceptance -DexcludeTags=isolated -Dchannel=UI -Denvironment=local -DchannelMode=static";
            Path = ".";
            TestReportPath = "build\reports\tests\test\index.html";
            TestInstallCommands = $null; },

        # === Acceptance Tests Isolated (stub) - API ===
        @{  Id = "acceptance-isolated-api";
            SampleTest = "shouldRecordPlacementTimestamp";
            Name = "latest - Acceptance Isolated (stub) - API";
            Command = "& .\gradlew.bat test -Dversion=latest -Dtype=acceptance -DincludeTags=isolated -Dchannel=API -Denvironment=local";
            Path = ".";
            TestReportPath = "build\reports\tests\test\index.html";
            TestInstallCommands = $null; },

        # === Acceptance Tests Isolated (stub) - UI ===
        @{  Id = "acceptance-isolated-ui";
            SampleTest = "shouldRecordPlacementTimestamp";
            Name = "latest - Acceptance Isolated (stub) - UI";
            Command = "& .\gradlew.bat test -Dversion=latest -Dtype=acceptance -DincludeTags=isolated -Dchannel=UI -Denvironment=local -DchannelMode=static";
            Path = ".";
            TestReportPath = "build\reports\tests\test\index.html";
            TestInstallCommands = $null; },

        # === Contract Tests (stub) ===
        @{  Id = "contract-stub";
            SampleTest = "shouldBeAbleToGetProduct";
            Name = "latest - Contract (stub)";
            Command = "& .\gradlew.bat test -Dversion=latest -Dtype=contract -DexcludeTags=isolated -DexternalSystemMode=stub -Denvironment=local";
            Path = ".";
            TestReportPath = "build\reports\tests\test\index.html";
            TestInstallCommands = $null; },

        # === Contract Tests Isolated (stub) ===
        @{  Id = "contract-stub-isolated";
            SampleTest = "shouldBeAbleToGetConfiguredTime";
            Name = "latest - Contract Isolated (stub)";
            Command = "& .\gradlew.bat test -Dversion=latest -Dtype=contract -DincludeTags=isolated -DexternalSystemMode=stub -Denvironment=local";
            Path = ".";
            TestReportPath = "build\reports\tests\test\index.html";
            TestInstallCommands = $null; },

        # === Contract Tests (real) ===
        @{  Id = "contract-real";
            SampleTest = "shouldBeAbleToGetProduct";
            Name = "latest - Contract (real)";
            Command = "& .\gradlew.bat test -Dversion=latest -Dtype=contract -DexternalSystemMode=real -Denvironment=local";
            Path = ".";
            TestReportPath = "build\reports\tests\test\index.html";
            TestInstallCommands = $null; },

        # === E2E Tests (real) - API ===
        @{  Id = "e2e-api";
            SampleTest = "shouldPlaceOrder";
            Name = "latest - E2E (real) - API";
            Command = "& .\gradlew.bat test -Dversion=latest -Dtype=e2e -Dchannel=API -Denvironment=local";
            Path = ".";
            TestReportPath = "build\reports\tests\test\index.html";
            TestInstallCommands = $null; },

        # === E2E Tests (real) - UI ===
        @{  Id = "e2e-ui";
            SampleTest = "shouldPlaceOrder";
            Name = "latest - E2E (real) - UI";
            Command = "& .\gradlew.bat test -Dversion=latest -Dtype=e2e -Dchannel=UI -Denvironment=local -DchannelMode=static";
            Path = ".";
            TestReportPath = "build\reports\tests\test\index.html";
            TestInstallCommands = $null; }

    )
}

# Export the configuration
return $Config
