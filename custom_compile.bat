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

rem DEBUG BUILD 
rem call csc.exe %files% -debug -warn:4 -d:DEBUG -lib:%opentkpath% -r:OpenTK.dll,System.Drawing.dll -out:..\build\bunnydemo.exe

rem RELEASE BUILD 
call csc.exe %files% -optimize -platform:x64 -target:exe -lib:%opentkpath% -r:OpenTK.dll,System.Drawing.dll -out:..\build\FCCCF_-_Lepus_Minor.exe

popd
IF NOT EXIST build\OpenTK.dll copy %opentkpath%\OpenTK.dll build\