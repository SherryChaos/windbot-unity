param(
    [switch]$Push,
    [switch]$SkipGenerate
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

Write-Host '[sync] fetch upstream (IceYGO/windbot)'
git fetch upstream
git checkout upm
if ($LASTEXITCODE -ne 0) { throw 'git checkout upm failed' }

Write-Host '[sync] merge upstream/master into upm'
git merge upstream/master --no-edit
if ($LASTEXITCODE -ne 0) {
    throw 'merge conflicts. Resolve them, then rerun with -SkipGenerate (or run git merge --continue), and regenerate resources manually.'
}

if (-not $SkipGenerate) {
    Write-Host '[sync] regenerate embedded decks/dialogs'
    python -X utf8 Tools/gen_windbot_resources.py
    if ($LASTEXITCODE -ne 0) { throw 'gen_windbot_resources.py failed' }

    Write-Host '[sync] regenerate/prune .meta'
    python -X utf8 Tools/gen_meta.py --prune
    if ($LASTEXITCODE -ne 0) { throw 'gen_meta.py failed' }
}

git add -A
git diff --cached --quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host '[sync] commit generated changes'
    git commit -m "sync upstream + regenerate windbot resources"
}

Write-Host '[sync] done. Verify:'
Write-Host '  msbuild WindBot.csproj /p:Configuration=Release /p:Platform=AnyCPU'
Write-Host '  dotnet build .dsh-diag/tmp/compilecheck/WindBotUnityCheck.csproj (if present)'
Write-Host 'Then bump the UPM pin in MasterDuelPro Packages/manifest.json.'

if ($Push) {
    git push origin upm
    Write-Host '[sync] pushed origin/upm'
}
