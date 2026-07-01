# Regenerate OpenAPI documents from the application model.
# Requires: .NET SDK. YAML step requires Python + PyYAML (pip install pyyaml).

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

$env:ASPNETCORE_ENVIRONMENT = "Testing"
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=hotels;Username=postgres;Password=postgres"

dotnet build "src/HotelSearch.Api/HotelSearch.Api.csproj" -p:OpenApiGenerateDocumentsOnBuild=true

python "scripts/openapi-json-to-yaml.py"

Write-Host "OpenAPI documents updated in docs/openapi/"
