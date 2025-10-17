param (
    [string]$TestSuiteDir,
    [string]$SourceBranchRef,
    [string]$TargetBranchRef,
    [string]$PullRequestId
)

$currentLocation = Get-Location
Set-Location $TestSuiteDir

$SourceBranchRef -match "refs/heads/(.+)"
$sourceBranch = $Matches[1]

$TargetBranchRef -match "refs/heads/(.+)"
$targetBranch = $Matches[1]

git branch -D $sourceBranch
git branch -D $targetBranch

git checkout $sourceBranch
git checkout $targetBranch
[array]$commitInfos = git log --format=format:"%h:%ae" $targetBranch..$sourceBranch

if ($commitInfos.Length -eq 0) {
    throw "There is no Git commit author."
}

Write-Host "$($commitInfos.Length) different commits found:"
$commitInfos | ForEach-Object { Write-Host $_ }

foreach ($commitInfo in $commitInfos) {
    $commitInfoArr = $commitInfo.Split(':')
    $commitHash = $commitInfoArr[0]
    $commitEmail = $commitInfoArr[1]
    if (-not $commitEmail.EndsWith("@microsoft.com")) {
        throw "Author email: $commitEmail of commit: $commitHash is not a valid Git commit author email."
    }
}

Write-Host "All emails are valid."

$prMergeBranch = "pull/$PullRequestId/merge"
git checkout $prMergeBranch

Set-Location $currentLocation