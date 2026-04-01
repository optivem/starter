# System Test Configuration (Shared)
# This file contains shared configuration values for Run-SystemTests.ps1

$Config = @{

    TestFilter = "--testNamePattern '<test>'"

    BuildCommands = @(
        @{  Name = "Install Dependencies";
            Command = "npm ci"
        }
    )

}

# Export the configuration
return $Config
