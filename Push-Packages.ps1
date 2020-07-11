$feed = "https://www.myget.org/F/tigra-astronomy/api/v2/package"
$symbolFeed = "https://www.myget.org/F/tigra-astronomy/api/v3/index.json"
Push-Location .\TA.Utils.Core\bin\Release
$packages = Get-ChildItem -Filter *.nupkg
foreach ($package in $packages)
{
    if ($package.Name -like "*.snupkg")
    {
        NuGet.exe push -Source $symbolFeed $package
    }
    else
    {
        NuGet.exe push -Source $feed $package
    }
}
Pop-Location