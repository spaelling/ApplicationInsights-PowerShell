# just make sure we can build and import module
Set-Location 'C:\Users\asp\OneDrive - Omada\git\ApplicationInsights-PowerShell'
Remove-Module ApplicationInsights_PowerShell -ErrorAction SilentlyContinue
dotnet clean
dotnet build

$Destination = New-Item -Path "$($env:TEMP)\ApplicationInsights_PowerShell_$((Get-Date).ToFileTimeUtc())" -ItemType Directory -Force
Copy-Item ".\bin\Debug\netstandard2.0\" -Destination $Destination -Recurse

try {
    Import-Module ("$Destination\netstandard2.0\ApplicationInsights_PowerShell.dll") -Force -ErrorAction Stop
}
catch {
    Write-Warning "Failed to import module`n$_"
}
Remove-Module ApplicationInsights_PowerShell -ErrorAction SilentlyContinue