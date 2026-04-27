# Bulk placeholder rename script for the shop template repo.
#
# Usage: dot-source or invoke with a `$Language` parameter; the script processes
# all matching files under the relevant subtrees and applies the ordered
# replacements for that language.
#
# Rationale: per `plans/PLACEHOLDER-RENAME.md`, surgical Edit calls are too
# expensive in Claude tokens for 1000s of matches. This script does the bulk
# find-replace in one pass, with allowlist protection and case-sensitive
# `-creplace` so we do not double-replace `Shop` -> `MyShop` -> `MyMyShop`.

param(
  [Parameter(Mandatory=$true)]
  [ValidateSet('typescript','dotnet','java')]
  [string]$Language
)

$root = Split-Path -Parent $PSScriptRoot

$config = @{
  typescript = @{
    Dirs = @(
      "$root\system\monolith\typescript",
      "$root\system\multitier\backend-typescript",
      "$root\system\multitier\frontend-react",
      "$root\system-test\typescript"
    )
    ExcludeDirs = @('node_modules','dist','build','playwright-report','test-results','coverage','.next')
    ExcludeFiles = @('package-lock.json','LICENSE')
    Extensions = @('*.ts','*.tsx','*.js','*.mjs','*.cjs','*.json','*.html','*.yml','*.yaml','*.properties','*.ps1','*.md','*.conf','Dockerfile','*.nginx')
  }
  dotnet = @{
    Dirs = @(
      "$root\system\monolith\dotnet",
      "$root\system\multitier\backend-dotnet",
      "$root\system-test\dotnet"
    )
    ExcludeDirs = @('node_modules','dist','bin','obj','build','coverage','TestResults','.vs')
    ExcludeFiles = @('LICENSE')
    Extensions = @('*.cs','*.cshtml','*.csproj','*.sln','*.slnx','*.json','*.ps1','*.yml','*.yaml','*.properties','*.md','Dockerfile','*.props','*.targets','*.editorconfig')
  }
  java = @{
    Dirs = @(
      "$root\system\monolith\java",
      "$root\system\multitier\backend-java",
      "$root\system-test\java"
    )
    ExcludeDirs = @('node_modules','dist','build','.gradle','target','out','bin')
    ExcludeFiles = @('LICENSE','gradlew','gradlew.bat')
    Extensions = @('*.java','*.gradle','*.kts','*.xml','*.properties','*.yml','*.yaml','*.json','*.ps1','*.md','Dockerfile')
  }
}

$cfg = $config[$Language]

$files = @()
foreach ($d in $cfg.Dirs) {
  if (Test-Path $d) {
    $files += Get-ChildItem -Path $d -Recurse -File | Where-Object {
      $p = $_.FullName
      $ok = $true
      foreach ($e in $cfg.ExcludeDirs) { if ($p -match "\\$e\\") { $ok = $false; break } }
      if ($ok -and ($cfg.ExcludeFiles -contains $_.Name)) { $ok = $false }
      if ($ok) {
        $matched = $false
        foreach ($ext in $cfg.Extensions) {
          if ($_.Name -like $ext) { $matched = $true; break }
        }
        $ok = $matched
      }
      $ok
    }
  }
}

Write-Output "[$Language] Files to process: $($files.Count)"

function Invoke-Replacements($text, $language) {
  $t = $text

  # ---- Protect allowlisted strings (sentinels) ----
  # eshop-tests is a different (archived) repo — not a placeholder.
  $t = $t -creplace 'eshop-tests','__SENT_ESHOP_TESTS__'
  $t = $t -creplace 'eshop_tests','__SENT_ESHOP_TESTS_SNAKE__'
  $t = $t -creplace '\beshop\b','__SENT_ESHOP__'

  # Publisher-real references
  $t = $t -creplace '@optivem/optivem-testing','__SENT_OPT_TEST__'
  $t = $t -creplace 'Optivem\.Testing','__SENT_OPT_TESTING_NS__'
  $t = $t -creplace 'com\.optivem\.testing','__SENT_COM_OPTIVEM_TESTING__'
  $t = $t -creplace 'com\.optivem:optivem-testing','__SENT_MAVEN_OPT_TESTING__'
  $t = $t -creplace 'ghcr\.io/optivem/shop','__SENT_GHCR_SHOP__'
  $t = $t -creplace 'optivem/actions','__SENT_OPTIVEM_ACTIONS__'
  # gh-optivem is the scaffolding CLI repo (publisher-real)
  $t = $t -creplace 'optivem/gh-optivem','__SENT_OPTIVEM_GH_OPTIVEM__'
  $t = $t -creplace 'gh-optivem','__SENT_GH_OPTIVEM__'
  # package.json author attribution
  $t = $t -creplace '"author":\s*"Optivem"','__SENT_AUTHOR__'

  # ---- Company replacements ----
  $t = $t -creplace '@optivem/','@my-company/'
  $t = $t -creplace 'api\.optivem\.com','api.my-company.example'

  # ---- .NET namespace compound ----
  $t = $t -creplace 'Optivem\.Shop','MyCompany.MyShop'
  # ---- Java package compound ----
  $t = $t -creplace 'com\.optivem\.shop','com.mycompany.myshop'

  # ---- Shop replacements (ordered, specific first) ----
  # Kebab / snake compounds
  $t = $t -creplace 'shop-','my-shop-'
  $t = $t -creplace 'shop_','my_shop_'
  # TypeScript directory segments
  if ($language -eq 'typescript') {
    $t = $t -creplace '/shop/','/myShop/'
  }
  # camelCase identifiers (e.g. shopDriver)
  $t = $t -creplace '\bshop([A-Z])','myShop$1'
  # _shop prefix + capital (e.g. _shopBrowser)
  $t = $t -creplace '_shop([A-Z])','_myShop$1'
  # Quoted literals
  if ($language -eq 'typescript') {
    $t = $t -creplace "'shop'","'myShop'"
    $t = $t -creplace '"shop"','"myShop"'
    $t = $t -creplace '"shop":','"myShop":'
    # Method calls and property access (TS convention: camelCase)
    $t = $t -creplace '\.shop\(','.myShop('
    $t = $t -creplace '\.shop\.','.myShop.'
    $t = $t -creplace '\.shop;','.myShop;'
    $t = $t -creplace '\.shop(\s|$|,|\))','.myShop$1'
    # Bare `shop: ` object keys
    $t = $t -creplace '(?m)^(\s*)shop:','$1myShop:'
    $t = $t -creplace '(\s|,|\{)shop:','$1myShop:'
    # `shop(` method definitions
    $t = $t -creplace '(?m)^(\s*)shop\(','$1myShop('
  }
  # SCREAMING
  $t = $t -creplace '\bSHOP\b','MY_SHOP'
  # PascalCase Shop (not preceded by `My`/`my` to avoid cascading)
  $t = $t -creplace '(?<![Mm]y)Shop','MyShop'

  # ---- Optivem remaining (standalone) ----
  $t = $t -creplace '\bOptivem\b','MyCompany'
  $t = $t -creplace '\bOPTIVEM\b','MY_COMPANY'
  $t = $t -creplace '\boptivem\b','my-company'

  # ---- Restore sentinels ----
  $t = $t -creplace '__SENT_OPT_TEST__','@optivem/optivem-testing'
  $t = $t -creplace '__SENT_OPT_TESTING_NS__','Optivem.Testing'
  $t = $t -creplace '__SENT_COM_OPTIVEM_TESTING__','com.optivem.testing'
  $t = $t -creplace '__SENT_MAVEN_OPT_TESTING__','com.optivem:optivem-testing'
  $t = $t -creplace '__SENT_GHCR_SHOP__','ghcr.io/optivem/shop'
  $t = $t -creplace '__SENT_OPTIVEM_ACTIONS__','optivem/actions'
  $t = $t -creplace '__SENT_AUTHOR__','"author": "Optivem"'
  $t = $t -creplace '__SENT_ESHOP_TESTS__','eshop-tests'
  $t = $t -creplace '__SENT_ESHOP_TESTS_SNAKE__','eshop_tests'
  $t = $t -creplace '__SENT_ESHOP__','eshop'

  return $t
}

$changed = 0
foreach ($f in $files) {
  $orig = Get-Content -Raw -LiteralPath $f.FullName -Encoding UTF8
  if ($null -eq $orig) { continue }
  $t = Invoke-Replacements $orig $Language
  if ($t -ne $orig) {
    [System.IO.File]::WriteAllText($f.FullName, $t, [System.Text.UTF8Encoding]::new($false))
    $changed++
  }
}
Write-Output "[$Language] Files changed: $changed / $($files.Count)"
