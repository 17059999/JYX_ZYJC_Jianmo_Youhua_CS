#############################################################################
## Copyright (c) Microsoft. All rights reserved.
##
## Microsoft Windows PowerShell Scripting
## File:    Merge-Branch.ps1
## Purpose: This script will be invoked by VSO agent to merge latest changes
##          to branches with specific prefix.
##
#############################################################################

#############################################################################
## Some test cases are windows-bug-related, we will not open source them
## until the bugs are fixed.
## Those test cases are in "bugfixing/<bug-number>" branches on VSO. To make
## merge into staging smoothly. This script will check whether there is a
## merge conflict every time staging branch is updated.
## Once staging is updated, VSO agent will invoke this script to merge the
## changes in staging into every "bugfixing" branch. If there are conflicts
## when merging, related authors will be notified through email.
##
## For example:
##                .-B-.  .--C1--C2--C3--.
##               /     \/                \
##              A1-----A2-----------------A3-----A4    <== staging
##                \      \                  \    /
##                 `-X1---X2-----------------X3-'      <== bugfixing/42
##
## Alice creates a branch based on commit A1, and creats some test cases
## covering a Windows bug, commits at X1. Meanwhile, Bob opens a pull request
## to merge his commit B into staging. B is merged then through commit A2.
## Now staging is updated, so the script will be invoked to merge A2 to
## bugfixing/42. If no conflicts, staging will be merged into bugfixing/42
## through commit X2.
## Commit X3 is similar. Everytime staging be updated, bugfixing branches
## will be updated too.
## Finally, a new Windows version is released and the bug is fixed. We are
## ready to open source the corresponding test case. Just open a pull request
## and the branch is merged into staging at commit A4.
#############################################################################

#----------------------------------------------------------------------------
# Parameters
# $RemoteName:       Another remote repo to be updated
# $RemoteURL:        Url of the to-be-updated repo
# $BranchPrefix:     Branch with this prefix will be auto merged to latest commit
# $MergeMessage:     Commit message
# $BranchesToMerge:  The branch list which needs to be merged as well as bugfix branches
#----------------------------------------------------------------------------
param (
    [Parameter(Mandatory=$True)][string]$RemoteName,
    [Parameter(Mandatory=$True)][string]$RemoteURL,
    [string]$BranchPrefix = "bugfixing",
    [string]$MergeMessage = "Sync with latest code.",
    [string[]]$BranchesToMerge
)

# VSO agent will checkout the specific commit 
# For example, after a pull request, the newest commit on staging is
# 3adbb931a2f4a8b482f743665ab1bcfe7eeb5c48.
# The agent will run
# git checkout --progress --force 3adbb931a2f4a8b482f743665ab1bcfe7eeb5c48
#
# Now save the commit hash. The commit hash will be used later in merging.
$Commit = git rev-parse --verify HEAD

# Record how many merge conflicts
$Global:Conflicts = 0
$Global:FailToPush = 0
$Global:Branches = 0

# Merge staging back to bugfixing branches
Function MergeChanges($BranchName)
{
    $RemoteBranch = "$RemoteName/$BranchName"
    $LocalBranch = "$BranchName"

    # Check if the branch is on local machine.
    # We cannot work on remote branch, need to create a local branch first.
    if (!(git branch | Select-String -Quiet $LocalBranch))
    {
        Write-Host "[merge] Cannot find $LocalBranch branch. Creating it."
        git branch --track $LocalBranch $RemoteBranch
    }

    # Switch to the bugfixing branch.
    Write-Host "[merge] Switch to branch $LocalBranch."
    git checkout $LocalBranch
    git reset --hard $RemoteBranch

    # Try merge staging to this branch.
    Write-Host "[merge] Merge with latest code."
    git merge --no-commit $Commit

    if ($LASTEXITCODE -eq 0)
    {
        # If succeed, just merge it and push to VSO.
        Write-Host "[merge] No conflict, merge it."
        git commit -m $MergeMessage
        git push $RemoteName ${LocalBranch}:${LocalBranch}

        if($LASTEXITCODE -ne 0) {
            $Global:FailToPush++
        }
    }
    else
    {
        # Oops cannot merge directly :(
        # Find the authors who committed on this branch, and notify them
        Write-Host "[merge] Conflict when merging with $Commit."
        git merge --abort

        $Global:Conflicts++
    }
}

# Adding an SSH remote to avoid typing password or token.
if (!(git remote | Select-String -Quiet $RemoteName))
{
    Write-Host "[merge] Cannot find $RemoteName remote. Adding it."
    git remote add $RemoteName $RemoteURL
}

# Check if the url of the SSH remote is correct.
# If not, setting it to the right one.
$CurrentRemoteUrl = git config --get remote.$RemoteName.url
if ($CurrentRemoteUrl -ne $RemoteURL)
{
    Write-Host "[merge] Current $RemoteName is to $CurrentRemoteUrl. Changing it to $RemoteURL."
    git remote set-url $RemoteName $RemoteURL
}

git fetch --prune $RemoteName

if ($LASTEXITCODE -ne 0) {
    Write-Host "##vso[task.LogIssue type=error;][merge] Unable to fetch $RemoteName."
    exit 1
}

# Find all branches with specific prefix.
# The branch name should be $BranchPrefix/<bug-number>
git branch -a | Select-String -pattern "remotes/$RemoteName/$BranchPrefix/(?<bugid>\d+)" | % {$_.matches} | % {$_.groups['bugid']} | ForEach-Object {
    Write-Host "===============Merge $BranchPrefix branches======================="
    Write-Host "[merge] Check branch $BranchPrefix/$_"
    MergeChanges $BranchPrefix/$_
    $Global:Branches++
}
Write-Host "============================================================"

# Merge other branches specified by the input param $BranchesToMerge
Write-Host "=================Merge other branches======================="
foreach ($BranchName in $BranchesToMerge)
{
    Write-Host "[merge] Check branch $BranchName"
    MergeChanges $BranchName
    $Global:Branches++
}
Write-Host "============================================================"

# Just exit here if there is no branches to merge
if ($Global:Branches -eq 0)
{
    exit 0
}
# If there are fail To push
if ($Global:FailToPush -gt 0)
{
    Write-Host "##vso[task.LogIssue type=error;][merge] $Global:FailToPush out of $Global:Branches branches have failed to push."
    exit 1
}

# If there are conflicts
if ($Global:Conflicts -gt 0)
{
    Write-Host "##vso[task.LogIssue type=error;][merge] $Global:Conflicts out of $Global:Branches branches have conflict while merging."
    exit 1
}

# All merged :)
Write-Host "[merge] $Commit has been merged to $Global:Branches branches without conflicts."
exit 0
