# System Test Configuration (Shared)
# This file contains shared configuration values for Run-SystemTests.ps1

$Config = @{

    TestFilter = "--tests '*.<test>'"

    BuildCommands = @(
        @{  Name = "Clean Build";
            Command = ".\gradlew.bat clean compileJava compileTestJava"
        }
    )

}

# Export the configuration
return $Config
