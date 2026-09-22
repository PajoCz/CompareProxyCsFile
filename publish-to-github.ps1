# GitHub Publish Script for CompareProxyCsFile
# Run this script from PowerShell

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "CompareProxyCsFile - GitHub Push" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Set Git path (Visual Studio Git)
$gitPath = "C:\Program Files\Microsoft Visual Studio\18\Professional\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\Git\cmd\git.exe"

# Check if Git exists
if (!(Test-Path $gitPath)) {
    Write-Host "ERROR: Git not found at: $gitPath" -ForegroundColor Red
    Write-Host "Please install Git or update the path in this script." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "You can also use Visual Studio's Git interface:" -ForegroundColor Yellow
    Write-Host "1. Open Solution in Visual Studio" -ForegroundColor White
    Write-Host "2. Team Explorer > Changes" -ForegroundColor White
    Write-Host "3. Commit all changes" -ForegroundColor White
    Write-Host "4. Push to origin/main" -ForegroundColor White
    pause
    exit
}

# Navigate to the repository root
Set-Location "C:\Users\pbalas\source\repos\CompareProxyCsFile"

Write-Host "Current directory: $(Get-Location)" -ForegroundColor Yellow
Write-Host "Using Git: $gitPath" -ForegroundColor Yellow
Write-Host ""

# Check Git status
Write-Host "1. Checking Git status..." -ForegroundColor Green
& $gitPath status

Write-Host ""
Write-Host "2. Adding all files to Git..." -ForegroundColor Green
& $gitPath add -A

Write-Host ""
Write-Host "3. Committing changes..." -ForegroundColor Green
& $gitPath commit -m "Initial commit: CompareProxyCsFile v1.0.0

Complete project with:
- Semantic proxy comparison using Roslyn
- Timestamped TXT reports
- Comprehensive documentation
- GitHub workflows and templates
- MIT License"

Write-Host ""
Write-Host "4. Checking current branch..." -ForegroundColor Green
& $gitPath branch

Write-Host ""
Write-Host "5. Renaming branch to 'main' (if needed)..." -ForegroundColor Green
& $gitPath branch -M main

Write-Host ""
Write-Host "6. Pushing to GitHub..." -ForegroundColor Green
& $gitPath push -u origin main

Write-Host ""
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "? DONE! Check your repository at:" -ForegroundColor Green
Write-Host "https://github.com/PajoCz/CompareProxyCsFile" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Go to GitHub repository settings" -ForegroundColor White
Write-Host "2. Add topics: csharp, proxy, comparison, wcf, roslyn, code-analysis, semantic, diff" -ForegroundColor White
Write-Host "3. Create first release (v1.0.0)" -ForegroundColor White
Write-Host "4. Upload compiled binaries to the release" -ForegroundColor White
Write-Host ""
Write-Host "See POST_PUBLICATION_CHECKLIST.md for more details." -ForegroundColor Yellow
Write-Host ""

pause
