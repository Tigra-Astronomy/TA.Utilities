# PowerShell script to cache NuGet packages for NCrunch workaround
# This script creates a temporary project and restores the required packages

Write-Host "NCrunch NuGet Package Cache Script" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan
Write-Host ""

# Define unique packages (deduplicated from the original list)
$packages = @(
    @{ Name = "System.Collections.NonGeneric"; Version = "4.3.0" }
    @{ Name = "System.Collections.Specialized"; Version = "4.3.0" }
    @{ Name = "System.CodeDom"; Version = "6.0.0" }
    @{ Name = "System.ComponentModel"; Version = "4.3.0" }
    @{ Name = "System.ComponentModel.Primitives"; Version = "4.3.0" }
    @{ Name = "System.ComponentModel.TypeConverter"; Version = "4.3.0" }
    @{ Name = "System.Diagnostics.TraceSource"; Version = "4.3.0" }
    @{ Name = "System.Management"; Version = "6.0.0" }
    @{ Name = "System.Runtime.CompilerServices.Unsafe"; Version = "6.0.0" }
    @{ Name = "System.Runtime.Loader"; Version = "4.0.0" }
    @{ Name = "System.Runtime.Loader"; Version = "4.3.0" }
    @{ Name = "System.Runtime.Serialization.Formatters"; Version = "4.3.0" }
    @{ Name = "System.Runtime.Serialization.Primitives"; Version = "4.3.0" }
    @{ Name = "System.Text.Encoding.CodePages"; Version = "6.0.0" }
    @{ Name = "System.Threading.Thread"; Version = "4.0.0" }
    @{ Name = "System.Threading.Thread"; Version = "4.3.0" }
    @{ Name = "Microsoft.NETFramework.ReferenceAssemblies.net20"; Version = "1.0.2" }
    @{ Name = "xunit.abstractions"; Version = "2.0.3" }
    @{ Name = "xunit.runner.utility"; Version = "2.4.1" }
)

# Get the script directory (solution root)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Write-Host "Solution directory: $scriptDir" -ForegroundColor Gray

# Create temporary directory under solution root to inherit nuget.config
$tempDir = Join-Path $scriptDir ".ncrunch-cache-temp"
if (Test-Path $tempDir) {
    Remove-Item -Path $tempDir -Recurse -Force
}
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
Write-Host "Created temporary directory: $tempDir" -ForegroundColor Gray
Write-Host ""

try {
    # Create a temporary .csproj file
    $projectFile = Join-Path $tempDir "TempCacheProject.csproj"
    
    $projectContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
"@

    foreach ($package in $packages) {
        $projectContent += "`n    <PackageReference Include=`"$($package.Name)`" Version=`"$($package.Version)`" />"
    }

    $projectContent += @"

  </ItemGroup>
</Project>
"@

    Set-Content -Path $projectFile -Value $projectContent
    Write-Host "Created temporary project file" -ForegroundColor Gray
    Write-Host ""

    # Restore packages
    Write-Host "Restoring $($packages.Count) unique NuGet packages..." -ForegroundColor Yellow
    Write-Host "This may take a few minutes..." -ForegroundColor Yellow
    Write-Host ""

    Push-Location $tempDir
    Write-Host "Running: dotnet restore" -ForegroundColor Gray
    Write-Host ""

    # Run restore with detailed output for troubleshooting
    dotnet restore --verbosity detailed
    $restoreExitCode = $LASTEXITCODE
    Pop-Location

    Write-Host ""
    if ($restoreExitCode -eq 0) {
        Write-Host "✓ Successfully cached all packages!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Packages are now available in your NuGet cache:" -ForegroundColor Green
        Write-Host "  $env:USERPROFILE\.nuget\packages" -ForegroundColor Gray
    }
    else {
        Write-Host "✗ Restore failed with exit code $restoreExitCode" -ForegroundColor Red
        Write-Host ""
        Write-Host "Common causes:" -ForegroundColor Yellow
        Write-Host "  1. Network connectivity issues" -ForegroundColor Gray
        Write-Host "  2. NuGet.org is unavailable" -ForegroundColor Gray
        Write-Host "  3. Package version no longer exists" -ForegroundColor Gray
        Write-Host "  4. .NET SDK not properly installed" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Try running: dotnet --info" -ForegroundColor Yellow
    }
}
finally {
    # Clean up temporary directory
    Write-Host ""
    Write-Host "Cleaning up temporary files..." -ForegroundColor Gray
    Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Done!" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "You can now reset NCrunch (NCrunch > Reset Engine) to use the cached packages." -ForegroundColor Cyan
