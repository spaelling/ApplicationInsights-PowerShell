<#
rebuilds the module, loads it, and does some simple testing

NOTE: read the README.md
#>

# read from file that is ignored by git
$InstrumentationKey = Get-Content "$PSScriptRoot\InstrumentationKey.txt"

Set-Location 'C:\Users\asp\OneDrive - Omada\git\ApplicationInsights-PowerShell'
Remove-Module ApplicationInsights_PowerShell -ErrorAction SilentlyContinue
dotnet clean
dotnet build
$Destination = New-Item -Path "$($env:TEMP)\ApplicationInsights_PowerShell_$((Get-Date).ToFileTimeUtc())" -ItemType Directory -Force
Copy-Item ".\bin\Debug\netstandard2.0\" -Destination $Destination -Recurse
#$Destination.FullName
$ModulePath = "$Destination\netstandard2.0\ApplicationInsights_PowerShell.dll"
try {
    Import-Module $ModulePath -Force -ErrorAction Stop
}
catch {
    Write-Warning "Failed to import module`n$_"
    return
}

Write-Host "Connecting telemetry client..."
Connect-ApplicationInsightsTelemetryClient -InstrumentationKey $InstrumentationKey -Verbose

$PSDefaultParameterValues = @{
    "Send-TrackEvent:Passthru" = $true
    "Send-TrackEvent:FunctionName" = "MyTestFunction"
}
"My event > Send-TrackEvent" | Send-TrackEvent | Write-Output

Write-Host "divide by zero error in script"
try {
    # divide by zero error
    1/0
}
catch {
    Send-TrackException -ErrorRecord $_ -MyInvocation $MyInvocation -Message "divide by zero error in script" -Verbose
}

function Test-Exception {
    param (
        [switch]$DivideByZero
    )
    
    if($DivideByZero.IsPresent)
    {
        try {
            # divede by zero error
            Write-Host "divide by zero error in function"
            1/0
        }
        catch {
            Send-TrackException $_ -MyInvocation $MyInvocation -Message "divide by zero error in function" -Verbose
        }
    }

}

Test-Exception -DivideByZero

Remove-Module $ModulePath -ErrorAction SilentlyContinue