# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           DevOpsLib.PSM1
# Purpose:        Library about wrapping pipeline related api.
# Requirements:   Windows Powershell 2.0
# Supported OS:   Windows Server 2008 R2, Windows Server 2012, Windows Server 2012 R2,
#                 Windows Server 2016 and later
#
##############################################################################

function Get-PipelineIdByName {
##
#.SYNOPSIS
# Get Pipeline Id By Pipleine name.
#.PARAMETER ApiUrl
# The ApiUrl.
#.PARAMETER AccessToken
# The access token.
#.PARAMETER PipelineName
# The pipeline name.
##
[CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$ApiUrl,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$AccessToken,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$PipelineName
    )
    process {
        $tokenBytes = [System.Text.Encoding]::UTF8.GetBytes(":$AccessToken")
        $authString = [System.Convert]::ToBase64String($tokenBytes)
        $authHeaders = @{"Authorization" = "Basic $authString" }
        $definitionsUrl = "$ApiUrl/build/definitions?api-version=5.1"
        $definitionsInfo = Invoke-RestMethod -Uri $definitionsUrl -Method Get -Headers $authHeaders

        $pipelineId = ($definitionsInfo.value | Where-Object {
                $_.name -eq $PipelineName
            } | Select-Object -Property id).id

        return $pipelineId
    }
}

function Get-PipelineDefinitions {
##
#.SYNOPSIS
# Get All Pipeline Definitions by ApiUrl.
#.PARAMETER ApiUrl
# The ApiUrl.
#.PARAMETER AccessToken
# The access token.
##
[CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$ApiUrl,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$AccessToken
    )
    process {
        $tokenBytes = [System.Text.Encoding]::UTF8.GetBytes(":$AccessToken")
        $authString = [System.Convert]::ToBase64String($tokenBytes)
        $authHeaders = @{"Authorization" = "Basic $authString" }
        $definitionsUrl = "$ApiUrl/build/definitions?api-version=5.1"
        $definitionsInfo = Invoke-RestMethod -Uri $definitionsUrl -Method Get -Headers $authHeaders

        return $definitionsInfo
    }
}

function Get-BuildInfoByBuildId {
##
#.SYNOPSIS
# Get build info by build id.
#.PARAMETER ApiUrl
# The ApiUrl.
#.PARAMETER AccessToken
# The access token.
#.PARAMETER BuildId
# The id of Build.
##
[CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$ApiUrl,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$AccessToken,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][int]$BuildId
    )
        
    process {
        $tokenBytes = [System.Text.Encoding]::UTF8.GetBytes(":$AccessToken")
        $authString = [System.Convert]::ToBase64String($tokenBytes)
        $authHeaders = @{"Authorization" = "Basic $authString" }
        $infoUrl = "$ApiUrl/build/builds/$($BuildId)?api-version=5.1"
        $buildInfo = Invoke-RestMethod -Uri $infoUrl -Method Get -Headers $authHeaders

        return $buildInfo
    }
}

function GetArtifactsByBuildId {
##
#.SYNOPSIS
# Get Artifacts by build id.
#.PARAMETER ApiUrl
# ApiUrl.
#.PARAMETER AccessToken
# AccessToken.
#.PARAMETER BuildId
# The Build Id.
#.PARAMETER ArtifactName
# The Artifact Name.
#.PARAMETER DownloadPath
# The Download Path.
##
[CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$ApiUrl,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$AccessToken,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][int]$BuildId,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$ArtifactName,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$DownloadPath
    )
    process {
        Add-Type -AssemblyName System.Net.Http
        Add-Type -AssemblyName System.IO.Compression.FileSystem

        $tokenBytes = [System.Text.Encoding]::UTF8.GetBytes(":$AccessToken")
        $authString = [System.Convert]::ToBase64String($tokenBytes)
        $authHeaders = @{"Authorization" = "Basic $authString" }

        Write-Host "Get Artifacts Address From Build:$BuildId"
        $artifactUrl = "$ApiUrl/build/builds/$($BuildId)/artifacts?artifactName=$($ArtifactName)&api-version=5.1"
        $artifactInfo = Invoke-RestMethod -Uri $artifactUrl -Method Get -Headers $authHeaders
        $downloadUrl = $artifactInfo.resource.downloadUrl        
        $rootPath = Get-Location
        $localFileName = "$rootPath/$ArtifactName.zip"

        try{
            Write-Host "Start download Artifact from $downloadUrl"
            $fileStream = [System.IO.File]::Create($localFileName)
            $httpClient = New-Object System.Net.Http.HttpClient
            $httpClient.DefaultRequestHeaders.Authorization = New-Object System.Net.Http.Headers.AuthenticationHeaderValue("Basic", $authString)
            $stream = $httpClient.GetStreamAsync($downloadUrl).GetAwaiter().GetResult()
            $fileStream.Seek(0, [System.IO.SeekOrigin]::Begin)
            $stream.CopyTo($fileStream)
            $stream.Close()
            $httpClient.Dispose()
            $fileStream.Close()
        }catch{
            $errorMessage = $_.Exception.Message
            Write-Host $errorMessage
        }

        Write-Host "Start Expand Archive:$localFileName"
        Expand-Archive -Path $localFileName -DestinationPath $DownloadPath -Force
        Write-Host "Download Artifacts completed"
        Remove-Item $localFileName -Force
    }
}

function Trigger-Build {
##
#.SYNOPSIS
# Trigger Build.
#.PARAMETER ApiUrl
# ApiUrl.
#.PARAMETER AccessToken
# AccessToken.
#.PARAMETER CurrBuildId
# The Current Build Id.
#.PARAMETER PipelineId
# The Pipeline Id.
#.PARAMETER JobParameters
# Job Parameters.
#.PARAMETER Reason
# The reason why the build is triggered.
#.PARAMETER TargetRepoBranch
# Target Repo Branch.
##
[CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$ApiUrl,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$AccessToken,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][int]$CurrBuildId,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][int]$PipelineId,
        [Parameter(Mandatory=$false)][string]$JobParameters,
        [Parameter(Mandatory=$false)][string]$Reason = "manual",
        [Parameter(Mandatory=$false)][string]$TargetRepoBranch
    )
    process {
        $tokenBytes = [System.Text.Encoding]::UTF8.GetBytes(":$AccessToken")
        $authString = [System.Convert]::ToBase64String($tokenBytes)
        $authHeaders = @{"Authorization" = "Basic $authString" }

        $triggerUrl = "$ApiUrl/build/builds?api-version=5.1"
        $selfInfo = Get-BuildInfoByBuildId -ApiUrl $ApiUrl -AccessToken $AccessToken -BuildId $CurrBuildId
        $parameters = if ([string]::IsNullOrEmpty($JobParameters)) { "" } else { "{$JobParameters}" }
        Write-Host "jobParameters: $parameters"

        $sourceBranch = $selfInfo.sourceBranch
        if($TargetRepoBranch -ne $null -and $TargetRepoBranch -ne [string]::Empty){
            $sourceBranch = $TargetRepoBranch            
        }

        Write-Host "sourceBranch: $sourceBranch"

        $body = @{
            definition    = @{
                id = $PipelineId
            }
            parameters    = $parameters
            reason        = $Reason
            requestedFor  = $selfInfo.requestedFor
            sourceBranch  = $sourceBranch
            triggerInfo   = if ($Reason -eq "manual") { @{} } else { $selfInfo.triggerInfo }
        } | ConvertTo-Json -Depth 100        

        $buildInfo = Invoke-RestMethod -Uri $triggerUrl -Method Post -Headers $authHeaders -Body $body -ContentType "application/json"
        [int]$buildId = $buildInfo.id
        $buildUrl = $buildInfo._links.web.href
        
        Write-Host "build Id: $buildId"
        Write-Host "build url: $buildUrl"
        Write-Host "==================== Trigger Completed ====================="

        if ($buildId -gt 0) {
            return $buildId
        }
        else {
            Write-Host "##vso[task.logissue type=error]Trigger build Failed."
            exit 1
        }
    }
}

function Cancel-BuildByBuildId {
##
#.SYNOPSIS
# Cancel Build By BuildId.
##
[CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$ApiUrl,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$AccessToken,
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][int]$BuildId
    )

    $tokenBytes = [System.Text.Encoding]::UTF8.GetBytes(":$AccessToken")
    $authString = [System.Convert]::ToBase64String($tokenBytes)
    $authHeaders = @{"Authorization" = "Basic $authString" }
    $infoUrl = "$ApiUrl/build/builds/$($BuildId)?api-version=5.1"
    $body = @{ status = 'cancelling' } | ConvertTo-Json

    try {
        Invoke-RestMethod -Uri $infoUrl -Method Patch -Body $body -Headers $authHeaders -ContentType "application/json"
    }
    catch {
        Write-Host "Failed to cancel build $BuildId"
    }
}

function Get-RunningPreBuildCount {
##
#.SYNOPSIS
# Get Current Running Build Count By Pipeline Id.
##
[CmdletBinding()]
    param(
		[Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$ApiUrl,
		[Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$AccessToken,
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][int]$PipelineId,
		[Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][int]$BuildId
    )

    $tokenBytes = [System.Text.Encoding]::UTF8.GetBytes(":$AccessToken")
    $authString = [System.Convert]::ToBase64String($tokenBytes)
    $authHeaders = @{"Authorization" = "Basic $authString" }
    $infoUrl = "$ApiUrl/build/builds?definitions=$PipelineId&statusFilter=inprogress&api-version=5.1"

    $currentList = Invoke-RestMethod -Uri $infoUrl -Method Get -Headers $authHeaders
 	$result = $currentList.value | Where-Object -Property id -LE $BuildId | Measure-Object
    return $result.Count
}

function Get-PipelineVariableDefaultValueByName {
##
#.SYNOPSIS
# Get the default value of a pipeline variable by its name.
##
[CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$ApiUrl,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$AccessToken,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][int]$PipelineId,
        [Parameter(Mandatory=$false)][string]$PipelineRevision = "latest",
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$VariableName
    )

    $tokenBytes = [System.Text.Encoding]::UTF8.GetBytes(":$AccessToken")
    $authString = [System.Convert]::ToBase64String($tokenBytes)
    $authHeaders = @{"Authorization" = "Basic $authString" }
    
    $revision = if ($PipelineRevision -eq "latest") {
        ""
    }
    else {
        "revision=$PipelineRevision&"
    }

    $infoUrl = "$ApiUrl/build/definitions/$($PipelineId)?$($revision)api-version=5.1"

    $pipelineDef = Invoke-RestMethod -Uri $infoUrl -Method Get -Headers $authHeaders
    return $pipelineDef.variables.$VariableName.value
}

# export all the functions in this module
Export-ModuleMember -Function *
