@echo off
cls
rem Builds demoparty. Call in the same directory as the source and build folders
setlocal
SET netDir=%1
set opentkpath=%2

PATH=%netDir%;%PATH%

setlocal ENABLEDELAYEDEXPANSION
for /f %%f in ('dir /b source') do (
	set "files=!files! %%f"
	)
pushd source
call csc.exe %files% -d:MUFFIN_PLATFORM_WINDOWS -d:DEBUG -lib:%opentkpath% -r:OpenTK.dll,System.Drawing.dll -out:..\build\demotest.exe
popd
IF NOT EXIST build\OpenTK.dll copy %opentkpath%\OpenTK.dll build\