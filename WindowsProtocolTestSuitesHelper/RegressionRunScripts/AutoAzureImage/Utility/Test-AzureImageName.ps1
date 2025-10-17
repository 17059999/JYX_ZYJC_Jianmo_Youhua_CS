###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

param(
	[string]$imageName
)

#--------------------------------------------------------------------------------------------------
# Script to test that a given Azure Image Name is valid
#--------------------------------------------------------------------------------------------------
function Test-AzureImageName([string]$azureImageName)
{
	if (-not ($azureImageName  -match  "^[^_\W][\w-._]{0,79}(?<![-.])$"))
	{
		Write-Host "Supplied AzureImageName does not match the pattern ^[^_\W][\w-._]{0,79}(?<![-.])$" -ForegroundColor Red
    }
    else 
    {
        Write-Host "Supplied AzureImageName is valid" -ForegroundColor Green
    }
}

Test-AzureImageName $imageName