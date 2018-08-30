<#
Example on usage

Assumes the module is already loaded
#>

if(-not (Get-Module ApplicationInsights_PowerShell))
{
    throw "please import module first"
}

if(-not $InstrumentationKey)
{
    $InstrumentationKey = Read-Host -Prompt "Enter instrumentation key"
}

# connect to AI
Connect-ApplicationInsightsTelemetryClient -InstrumentationKey $InstrumentationKey -Verbose

function MyFaultyFunction {
    param (
        
    )
    
    try {
        # divede by zero error
        1/0
    }
    catch {
        Send-TrackException -ErrorRecord $_ -MyInvocation $MyInvocation -Message "custom message goes here" -Verbose
        # minimal info is the ErrorRecord
        Send-TrackException -ErrorRecord $_
    }
}
MyFaultyFunction