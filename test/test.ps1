<#
just for looking into exceptions and other data available
#>

function Test-Exception {
    [CmdletBinding()]
    param (
        
    )
    
    begin {
    }
    
    process {
        try {
            # divede by zero error
            1/0
        }
        catch {
            $_.Exception | gm
            #$_.InvocationInfo
        }
    }
    
    end {
    }
}

Test-Exception