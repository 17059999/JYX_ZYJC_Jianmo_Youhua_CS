param (
    [string]$TestSuitePath,
    [string]$IsCheckAll = "false",
    [string]$targetBranch = "staging",
    [string]$sourceBranch
)

$InvocationPath = Split-Path $MyInvocation.MyCommand.Definition -parent
Write-Host "InvocationPath: $InvocationPath"
Write-Host "TestSuitePath:$TestSuitePath"
Write-Host "targetBranch:$targetBranch"
Write-Host "sourceBranch:$sourceBranch"

$cSharpSharedAssemblyFilePath = Resolve-Path "$TestSuitePath\AssemblyInfo\SharedAssemblyInfo.cs"
$cSharpPTMAssemblyFilePath = Resolve-Path "$TestSuitePath\AssemblyInfo\PTMAssemblyInfo.cs"
$cSharpPTMDetectorAssemblyFilePath = Resolve-Path "$TestSuitePath\AssemblyInfo\PTMDetectorAssemblyInfo.cs"
$cPlusPlusSharedAssemblyFilePath = Resolve-Path "$TestSuitePath\AssemblyInfo\resource.h"

Push-Location $TestSuitePath
$Diff = git diff --name-only "$targetBranch...$sourceBranch"
Pop-Location

$projectFileExtension = ".csproj", ".vcxproj"

$invalidProjectFileList = New-Object 'System.Collections.Generic.List[String]'

function Verify-ProjectFile {
    param (
        [string]$filename,
        [string]$fileFullPath,
        [xml]$content
    )
    switch ($filename.Substring($filename.LastIndexOf('.'))) {
        ".csproj" {
            Verify-CSharpProjectFile -filename $filename -fileFullPath $fileFullPath -content $content
        }
        ".vcxproj" {
            Verify-VCXProjectFile -filename $filename -fileFullPath $fileFullPath -content $content
        }
    }
}

function Verify-CSharpProjectFile {
    param (
        [string]$filename,
        [string]$fileFullPath,
        [xml]$content
    )

    try {
        # Get Assembly file path
        $assemblyFilePath = ""
        [array]$itemsGroupNodes = $content.Project.ItemGroup
        foreach ($itemGroup in $itemsGroupNodes) {
            [array]$compileNodes = $itemGroup.Compile
            if ($compileNodes -ne $null) {
                foreach ($compileNode in $compileNodes) {
                    if (($compileNode.Include -ne $null) -and (($compileNode.Include.Contains("SharedAssemblyInfo.cs") -eq $TRUE) -or ($compileNode.Include.Contains("PTMAssemblyInfo.cs") -eq $TRUE) -or ($compileNode.Include.Contains("PTMDetectorAssemblyInfo.cs") -eq $TRUE))) {
                        $assemblyFilePath = $compileNode.Include
                        Write-Host "Assembly File Path:  $assemblyFilePath"
                        break
                    }
                }
            }
        }

        $projectFilePath = [System.IO.Path]::GetDirectoryName($fileFullPath)
        Write-Host "Project File Path:  $projectFilePath"

        $assemblyFullFilePath = Resolve-Path "$projectFilePath\$assemblyFilePath"

        if (($assemblyFullFilePath.Path -ne $cSharpSharedAssemblyFilePath.Path) -and ($assemblyFullFilePath.Path -ne $cSharpPTMAssemblyFilePath.Path) -and ($assemblyFullFilePath.Path -ne $cSharpPTMDetectorAssemblyFilePath.Path)) {
            $invalidProjectFileList.Add($filename)
        }
    }
    catch {
        Write-Host $_.Exception.Message
        $invalidProjectFileList.Add($filename)
    }
}

function Verify-VCXProjectFile {
    param (
        [string]$filename,
        [string]$fileFullPath,
        [xml]$content
    )

    try {
        # Get Assembly file path
        $assemblyFilePath = ""
        [array]$itemsGroupNodes = $content.Project.ItemGroup
        foreach ($itemGroup in $itemsGroupNodes) {
            [array]$compileNodes = $itemGroup.ClInclude
            if ($compileNodes -ne $null) {
                foreach ($compileNode in $compileNodes) {
                    if (($compileNode.Include -ne $null) -and ($compileNode.Include.Contains("resource.h") -eq $TRUE)) {
                        $assemblyFilePath = $compileNode.Include
                        Write-Host "Assembly File Path:  $assemblyFilePath"
                        break
                    }
                }
            }
        }

        # Get Assembly file content
        $projectFilePath = [System.IO.Path]::GetDirectoryName($fileFullPath)
        Write-Host "Project File Path:  $projectFilePath"

        $assemblyFullFilePath = Resolve-Path "$projectFilePath\$assemblyFilePath"

        if ($assemblyFullFilePath.Path -ne $cPlusPlusSharedAssemblyFilePath.Path) {
            $invalidProjectFileList.Add($filename)
        }
    }
    catch {
        Write-Host $_.Exception.Message
        $invalidProjectFileList.Add($filename)
    }
}

# Check whether the project file contain the assembly info
if ($IsCheckAll -eq "true") {
    Write-Host "Check whether all project files contain the assembly info"
    Get-ChildItem $TestSuitePath -Exclude "_Helper" -Recurse | ForEach-Object {       
        if ($projectFileExtension.Contains($_.Extension.ToLower())) {
            Write-Host "Check file: $_"
            [xml]$content = Get-Content $_.FullName -Raw
            $projectFileFullPath = "$TestSuitePath\$_"
            Verify-ProjectFile -filename $_ -fileFullPath $projectFileFullPath -content $content
        }
    }
}
else {
    Write-Host "Check whether the different project files contain the assembly info"
    $Diff | ForEach-Object {
        $file = $_.Trim()
        if (Test-Path "$TestSuitePath\$file") {
            $projectFileFullPath = "$TestSuitePath\$file"
            Write-Host "Check file: $projectFileFullPath"

            if ($file.Contains('.') -and $projectFileExtension.Contains($file.SubString($file.LastIndexOf('.')).Trim().ToLower())) {
                [xml]$content = Get-Content $projectFileFullPath -Raw
                Verify-ProjectFile -filename $file -fileFullPath $projectFileFullPath -content $content
            }
        }
    }
}

if ($invalidProjectFileList.Count -gt 0) {
    Write-Host "==========================================================="
    Write-Host "Project files found do not have the assembly info reference."
    $invalidProjectFileList
    Write-Host "==========================================================="
    throw 
}
else {
    Write-Host "Project files have the correct assembly info reference: no project file changed or all the project files contain the assembly info reference."
}