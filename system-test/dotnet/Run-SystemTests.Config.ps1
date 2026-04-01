# System Test Configuration (Shared)
# This file contains shared configuration values for Run-SystemTests.ps1

$Config = @{

    TestFilter = "--filter 'DisplayName~<test>'"

    BuildCommands = @(
        @{  Name = "Clean Build";
            Command = "dotnet clean; dotnet build"
        }
    )

}

# Export the configuration
return $Config
