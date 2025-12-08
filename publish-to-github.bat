@echo off
echo ==================================
echo CompareProxyCsFile - GitHub Push
echo ==================================
echo.

set "GIT_PATH=C:\Program Files\Microsoft Visual Studio\18\Professional\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\Git\cmd\git.exe"

if not exist "%GIT_PATH%" (
    echo ERROR: Git not found at: %GIT_PATH%
    echo.
    echo Please use Visual Studio's Git interface:
    echo 1. Open Solution in Visual Studio
    echo 2. Team Explorer ^> Changes
    echo 3. Commit all changes
    echo 4. Push to origin/main
    echo.
    pause
    exit /b
)

cd /d C:\Users\pbalas\source\repos\CompareProxyCsFile

echo 1. Checking Git status...
"%GIT_PATH%" status
echo.

echo 2. Adding all files to Git...
"%GIT_PATH%" add -A
echo.

echo 3. Committing changes...
"%GIT_PATH%" commit -m "Initial commit: CompareProxyCsFile v1.0.0"
echo.

echo 4. Checking current branch...
"%GIT_PATH%" branch
echo.

echo 5. Renaming branch to 'main' (if needed)...
"%GIT_PATH%" branch -M main
echo.

echo 6. Pushing to GitHub...
"%GIT_PATH%" push -u origin main
echo.

echo ==================================
echo DONE! Check your repository at:
echo https://github.com/PajoCz/CompareProxyCsFile
echo ==================================
echo.

echo See POST_PUBLICATION_CHECKLIST.md for next steps.
echo.

pause
