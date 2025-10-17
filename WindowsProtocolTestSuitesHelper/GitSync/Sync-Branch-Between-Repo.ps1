#############################################################################
## Copyright (c) Microsoft. All rights reserved.
##
## Microsoft Windows PowerShell Scripting
## File:    Sync-Branch-Between-Repo.ps1
## Purpose: This script will be invoked by VSO agent to push updates on a
##          specific branch to the same name branch on another repo.
##
#############################################################################

#############################################################################
## staging on VSO or GitHub could be updated when pull requests are merged.
## We need to make sure updates on one repo are synced to another one.
##
## In the following example, pull request is merged on VSO, and the VSO agent
## will call this script. The command is:
##
## Sync-Branch-Between-Repo.ps1
##   -RemoteName github
##   -RemoteUrl git@github.com:Microsoft/WindowsProtocolTestSuites.git
##   -SyncBranch staging
##   -MirrorBranch mirror
##
## Some team member, say Alice, fixes a bug and the codes are at her branch.
## After code review, changes should be merged into staging on VSO. So she
## creates a pull request to check in her code. Then the admin merges this
## pull request. Now the git history looks like:
##
##                       .-B-.
##                      /     \
##              A1-----A2------A3
##
## A1 is just an old commit. Before merging, VSO/staging and GitHub/staging
## are both at A2. Alice fixes a bug and makes a commit B on her branch.
## The pull request is merged through commit A3.
## Now VSO/staging is at A3, while GitHub/staging is at A2.
## So the script need to push the changes on VSO to GitHub.
##
## The script will do:
##   1. Record the current commit point, which is A3.
##   2. Checkout the GitHub staging branch to a local branch "mirror".
##      The name must be different from staging to avoid conflict.
##   3. Make a fast-forward commit on "mirror" branch to A3.
##   4. Push the "mirror" branch to GitHub. Now GitHub/staging is at A3.
#############################################################################

#----------------------------------------------------------------------------
# Parameters
# $RemoteName:     Another remote repo to be updated
# $RemoteUrl:      Url of the to-be-updated repo
# $SyncBranch:     Name of branch which is to be synced
# $MirrorBranch:   Name of local mirror branch, must be different from
#                  $SyncBranch to avoid conflict
#----------------------------------------------------------------------------
param (
    [Parameter(Mandatory = $True)][string]$RemoteName,
    [Parameter(Mandatory = $True)][string]$RemoteUrl,
    [Parameter(Mandatory = $True)][string]$SyncBranch,
    [Parameter(Mandatory = $True)][string]$MirrorBranch,
    [Parameter(Mandatory = $False)][string[]]$FilesToDelete = @("NuGet.config")
)

# Return the commit author and subject
Function CommitInfo($Hash) {
    $info += git show -s --format="%h by %an <%ae>:" $Commit
    $info += "`n"
    $info += git show -s --format="%s" $Commit
    return $info
}

# STEP 1
# VSO agent will checkout the specific commit 
# For example, after a pull request, the newest commit on staging is
# 3adbb931a2f4a8b482f743665ab1bcfe7eeb5c48.
# The agent will run
# git checkout --progress --force 3adbb931a2f4a8b482f743665ab1bcfe7eeb5c48
#
# Now save the commit hash. The commit hash will be used later in making
# fast-forward merge.
$Commit = git rev-parse --verify HEAD

# Check if the syncing-to repo is added as another git remote.
# If not, adding it.
if (!(git remote | Select-String -Quiet $RemoteName)) {
    Write-Host "[sync] Cannot find $RemoteName remote. Adding it."
    git remote add $RemoteName $RemoteUrl
}

# Check if the url of the syncing-to repo is correct.
# If not, setting it to the right one.
$CurrentRemoteUrl = git config --get remote.$RemoteName.url
if ($CurrentRemoteUrl -ne $RemoteUrl) {
    Write-Host "[sync] Current $RemoteName is to $CurrentRemoteUrl. Changing it to $RemoteUrl."
    git remote set-url $RemoteName $RemoteUrl
}

# Fetch the content on the syncing-to repo.
Write-Host "[sync] Fetch content on $RemoteName."
git fetch --prune $RemoteName

if ($LASTEXITCODE -ne 0) {
    Write-Host  "##vso[task.LogIssue type=error;][sync] Unable to fetch $RemoteName."
    exit 1
}

# Check if the mirror branch is created at agent machine.
# If not, creating it.
if (!(git branch | Select-String -Quiet $MirrorBranch)) {
    Write-Host "[sync] Cannot find $MirrorBranch branch. Creating it."
    git branch --track $MirrorBranch $RemoteName/$SyncBranch
}

# STEP 2
# Switch to the local mirror branch.
git checkout $MirrorBranch
git reset --hard $RemoteName/$SyncBranch

# Check if the syncing-to repo is already at the newest commit.
#
# For example:
# VSO is updated through pull request, then the script updates GitHub. This
# would be fine, GitHub is behind VSO.
# Once GitHub is updated, the agent is calling this script to check if we need
# to push updates on GitHub to VSO. However, at this moment, the two repos are
# in sync, so just stop here, no need to make a fast-forward merge and push.
$Current = git rev-parse --verify HEAD
if ($Current -eq $Commit) {
    Write-Host "[sync] $RemoteName/$SyncBranch is updated with lastest commit $Commit."
    exit 0
}

# Check if there are files to be deleted.
# If yes, delete them. 
if ($null -ne $FilesToDelete -and $FilesToDelete.Length -gt 0) {
    foreach ($file in $FilesToDelete) {
        if (git ls-tree -r $RemoteName/$SyncBranch --name-only | Select-String -Quiet $file) {
            Write-Host "[sync] Removing $file as it exists in the remote repository."
            git rm --cached $file
        }
        else {
            Write-Host "[sync] File $file does not exist in remote, skipping deletion."
        }
    }

    if (git diff --cached --quiet) {
        Write-Host "[sync] No files removed, skipping commit."
    }
    else {
        git commit -m "Delete excluded files using merge strategy"
    }
}

# STEP 3
# Make a fast-forward merge.
# Because the syncing-to repo is fall behind, a fast-forward merge should
# always succeed.
Write-Host "[sync] Fast forward to $Commit."
git merge --no-ff -m "Merging changes" $Commit
if ($LASTEXITCODE -ne 0) {
    Write-Host "[sync] Rebase needed due to divergence."
    git rebase $RemoteName/$SyncBranch
    if ($LASTEXITCODE -ne 0) {
        Write-Host "##vso[task.LogIssue type=error;][sync] Cannot rebase changes."
        exit 1
    }
}

# STEP 4
# Push the changes to the syncing-to repo. Done!
Write-Host "[sync] Push changes to $RemoteName."
git push $RemoteName ${MirrorBranch}:${SyncBranch}
