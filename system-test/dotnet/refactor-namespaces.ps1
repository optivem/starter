# Namespace refactoring script
# Transforms namespaces according to the refactoring plan

param(
    [string]$WorkspacePath = "."
)

Set-Location $WorkspacePath

$filesUpdated = @()
$transformCount = @{}

# Define transformation rules
# Format: @{ pattern = regex pattern; replacement = replacement string; filesFilter = filter for which files to apply; skipDirs = directories to skip }
$transformations = @(
    # DriverImpl transformations
    @{
        pattern = 'Driver\.Impl\.Commons'
        replacement = 'Driver.Core.Commons'
        includeFilter = { $_ -like '*\DriverImpl\*' }
    },
    @{
        pattern = 'using Optivem\.EShop\.SystemTest\.Infra\.'
        replacement = 'using Optivem.EShop.SystemTest.Driver.Core.'
        includeFilter = { $_ -like '*\DriverImpl\*' }
    },
    @{
        pattern = 'namespace Optivem\.EShop\.SystemTest\.Infra\.'
        replacement = 'namespace Optivem.EShop.SystemTest.Driver.Core.'
        includeFilter = { $_ -like '*\DriverImpl\*' }
    },
    @{
        pattern = 'using Optivem\.EShop\.SystemTest\.Core\.Clock\.'
        replacement = 'using Optivem.EShop.SystemTest.Driver.Core.Clock.'
        includeFilter = { $_ -like '*\DriverImpl\*' }
    },
    @{
        pattern = 'namespace Optivem\.EShop\.SystemTest\.Core\.Clock\.'
        replacement = 'namespace Optivem.EShop.SystemTest.Driver.Core.Clock.'
        includeFilter = { $_ -like '*\DriverImpl\*' }
    },
    @{
        pattern = 'using Optivem\.EShop\.SystemTest\.Core\.Clock$'
        replacement = 'using Optivem.EShop.SystemTest.Driver.Core.Clock'
        includeFilter = { $_ -like '*\DriverImpl\*' }
    },
    @{
        pattern = 'namespace Optivem\.EShop\.SystemTest\.Core\.Clock$'
        replacement = 'namespace Optivem.EShop.SystemTest.Driver.Core.Clock'
        includeFilter = { $_ -like '*\DriverImpl\*' }
    },
    @{
        pattern = 'using Optivem\.EShop\.SystemTest\.Core\.Tax\.'
        replacement = 'using Optivem.EShop.SystemTest.Driver.Core.Tax.'
        includeFilter = { $_ -like '*\DriverImpl\*' }
    },
    @{
        pattern = 'namespace Optivem\.EShop\.SystemTest\.Core\.Tax\.'
        replacement = 'namespace Optivem.EShop.SystemTest.Driver.Core.Tax.'
        includeFilter = { $_ -like '*\DriverImpl\*' }
    },
    @{
        pattern = 'using Optivem\.EShop\.SystemTest\.Core\.Tax$'
        replacement = 'using Optivem.EShop.SystemTest.Driver.Core.Tax'
        includeFilter = { $_ -like '*\DriverImpl\*' }
    },
    @{
        pattern = 'namespace Optivem\.EShop\.SystemTest\.Core\.Tax$'
        replacement = 'namespace Optivem.EShop.SystemTest.Driver.Core.Tax'
        includeFilter = { $_ -like '*\DriverImpl\*' }
    },
    @{
        pattern = 'using Optivem\.EShop\.SystemTest\.Core\.Shop\.'
        replacement = 'using Optivem.EShop.SystemTest.Driver.Core.Shop.'
        includeFilter = { $_ -like '*\DriverImpl\*' }
    },
    @{
        pattern = 'namespace Optivem\.EShop\.SystemTest\.Core\.Shop\.'
        replacement = 'namespace Optivem.EShop.SystemTest.Driver.Core.Shop.'
        includeFilter = { $_ -like '*\DriverImpl\*' }
    },
    @{
        pattern = 'using Optivem\.EShop\.SystemTest\.Core\.Shop$'
        replacement = 'using Optivem.EShop.SystemTest.Driver.Core.Shop'
        includeFilter = { $_ -like '*\DriverImpl\*' }
    },
    @{
        pattern = 'namespace Optivem\.EShop\.SystemTest\.Core\.Shop$'
        replacement = 'namespace Optivem.EShop.SystemTest.Driver.Core.Shop'
        includeFilter = { $_ -like '*\DriverImpl\*' }
    },
    
    # DriverPorts transformations
    @{
        pattern = 'Optivem\.EShop\.SystemTest\.Driver\.Ports'
        replacement = 'Optivem.EShop.SystemTest.Driver.Api'
        includeFilter = { $_ -like '*\Driver.Api\*' }
    },
    
    # DslImpl transformations
    @{
        pattern = 'using Optivem\.EShop\.SystemTest\.Core\.Clock\.'
        replacement = 'using Optivem.EShop.SystemTest.Dsl.Clock.'
        includeFilter = { $_ -like '*\Dsl.Core\*' }
    },
    @{
        pattern = 'namespace Optivem\.EShop\.SystemTest\.Core\.Clock\.'
        replacement = 'namespace Optivem.EShop.SystemTest.Dsl.Clock.'
        includeFilter = { $_ -like '*\Dsl.Core\*' }
    },
    @{
        pattern = 'using Optivem\.EShop\.SystemTest\.Core\.Clock$'
        replacement = 'using Optivem.EShop.SystemTest.Dsl.Clock'
        includeFilter = { $_ -like '*\Dsl.Core\*' }
    },
    @{
        pattern = 'namespace Optivem\.EShop\.SystemTest\.Core\.Clock$'
        replacement = 'namespace Optivem.EShop.SystemTest.Dsl.Clock'
        includeFilter = { $_ -like '*\Dsl.Core\*' }
    },
    @{
        pattern = 'using Optivem\.EShop\.SystemTest\.Core\.Erp\.'
        replacement = 'using Optivem.EShop.SystemTest.Dsl.Erp.'
        includeFilter = { $_ -like '*\Dsl.Core\*' }
    },
    @{
        pattern = 'namespace Optivem\.EShop\.SystemTest\.Core\.Erp\.'
        replacement = 'namespace Optivem.EShop.SystemTest.Dsl.Erp.'
        includeFilter = { $_ -like '*\Dsl.Core\*' }
    },
    @{
        pattern = 'using Optivem\.EShop\.SystemTest\.Core\.Erp$'
        replacement = 'using Optivem.EShop.SystemTest.Dsl.Erp'
        includeFilter = { $_ -like '*\\Dsl.Core\\*' }
    },
    @{
        pattern = 'namespace Optivem\.EShop\.SystemTest\.Core\.Erp$'
        replacement = 'namespace Optivem.EShop.SystemTest.Dsl.Erp'
        includeFilter = { $_ -like '*\\Dsl.Core\\*' }
    },
    @{
        pattern = 'using Optivem\.EShop\.SystemTest\.Core\.Shop\.'
        replacement = 'using Optivem.EShop.SystemTest.Dsl.Shop.'
        includeFilter = { $_ -like '*\\Dsl.Core\\*' }
    },
    @{
        pattern = 'namespace Optivem\.EShop\.SystemTest\.Core\.Shop\.'
        replacement = 'namespace Optivem.EShop.SystemTest.Dsl.Shop.'
        includeFilter = { $_ -like '*\\Dsl.Core\\*' }
    },
    @{
        pattern = 'using Optivem\.EShop\.SystemTest\.Core\.Shop$'
        replacement = 'using Optivem.EShop.SystemTest.Dsl.Shop'
        includeFilter = { $_ -like '*\\Dsl.Core\\*' }
    },
    @{
        pattern = 'namespace Optivem\.EShop\.SystemTest\.Core\.Shop$'
        replacement = 'namespace Optivem.EShop.SystemTest.Dsl.Shop'
        includeFilter = { $_ -like '*\\Dsl.Core\\*' }
    },
    @{
        pattern = 'using Optivem\.EShop\.SystemTest\.Core\.Tax\.'
        replacement = 'using Optivem.EShop.SystemTest.Dsl.Tax.'
        includeFilter = { $_ -like '*\\Dsl.Core\\*' }
    },
    @{
        pattern = 'namespace Optivem\.EShop\.SystemTest\.Core\.Tax\.'
        replacement = 'namespace Optivem.EShop.SystemTest.Dsl.Tax.'
        includeFilter = { $_ -like '*\\Dsl.Core\\*' }
    },
    @{
        pattern = 'using Optivem\.EShop\.SystemTest\.Core\.Tax$'
        replacement = 'using Optivem.EShop.SystemTest.Dsl.Tax'
        includeFilter = { $_ -like '*\\Dsl.Core\\*' }
    },
    @{
        pattern = 'namespace Optivem\.EShop\.SystemTest\.Core\.Tax$'
        replacement = 'namespace Optivem.EShop.SystemTest.Dsl.Tax'
        includeFilter = { $_ -like '*\\Dsl.Core\\*' }
    },
    @{
        pattern = 'using Optivem\.EShop\.SystemTest\.Core\.Gherkin\.'
        replacement = 'using Optivem.EShop.SystemTest.Scenario.'
        includeFilter = { $_ -like '*\\Dsl.Core\\*' }
    },
    @{
        pattern = 'namespace Optivem\.EShop\.SystemTest\.Core\.Gherkin\.'
        replacement = 'namespace Optivem.EShop.SystemTest.Scenario.'
        includeFilter = { $_ -like '*\\Dsl.Core\\*' }
    },
    @{
        pattern = 'using Optivem\.EShop\.SystemTest\.Core\.Gherkin$'
        replacement = 'using Optivem.EShop.SystemTest.Scenario'
        includeFilter = { $_ -like '*\\Dsl.Core\\*' }
    },
    @{
        pattern = 'namespace Optivem\.EShop\.SystemTest\.Core\.Gherkin$'
        replacement = 'namespace Optivem.EShop.SystemTest.Scenario'
        includeFilter = { $_ -like '*\\Dsl.Core\\*' }
    },
    @{
        pattern = 'using Optivem\.EShop\.SystemTest\.Core$'
        replacement = 'using Optivem.EShop.SystemTest.Dsl'
        includeFilter = { $_ -like '*\\Dsl.Core\\*' }
    },
    @{
        pattern = 'namespace Optivem\.EShop\.SystemTest\.Core$'
        replacement = 'namespace Optivem.EShop.SystemTest.Dsl'
        includeFilter = { $_ -like '*\\Dsl.Core\\*' }
    },
    @{
        pattern = 'DslImpl\.Gherkin'
        replacement = 'Scenario'
        includeFilter = { $_ -like '*\\Dsl.Core\\*' }
    },
    @{
        pattern = 'DslImpl\.'
        replacement = 'Dsl.' 
        includeFilter = { $_ -like '*\\Dsl.Core\\*' }
    }
)

# Get all C# files (excluding bin/obj folders)
$allCSFiles = Get-ChildItem -Path DriverImpl, DriverPorts, DslImpl -Recurse -Filter '*.cs' -File | 
              Where-Object { $_.FullName -notlike '*\bin\*' -and $_.FullName -notlike '*\obj\*' }

Write-Host "Processing $($allCSFiles.Count) C# files..."

foreach ($file in $allCSFiles) {
    $originalContent = Get-Content $file.FullName -Raw
    $content = $originalContent
    
    foreach ($transform in $transformations) {
        if (& $transform.includeFilter $file.FullName) {
            $newContent = $content -replace $transform.pattern, $transform.replacement
            if ($newContent -ne $content) {
                $transformCount[$transform.pattern] = ($transformCount[$transform.pattern] ?? 0) + 1
                $content = $newContent
            }
        }
    }
    
    if ($content -ne $originalContent) {
        Set-Content $file.FullName -Value $content -NoNewline
        $filesUpdated += $file.FullName
    }
}

Write-Host ""
Write-Host "=== Refactoring Complete ==="
Write-Host "Total files updated: $($filesUpdated.Count)"
Write-Host ""
Write-Host "Sample files updated:"
$filesUpdated | Select-Object -First 5 | ForEach-Object { Write-Host "  $_" }
Write-Host ""
Write-Host "Transformation statistics:"
$transformCount.GetEnumerator() | Where-Object { $_.Value -gt 0 } | Sort-Object Value -Descending | ForEach-Object {
    Write-Host "  $($_.Key): $($_.Value) occurrences"
}

exit 0
