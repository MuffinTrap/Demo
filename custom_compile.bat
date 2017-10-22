@echo off
cls
rem Builds demoparty. Call in the same directory as the source and build folders

SET netDir=%1
set opentkpath=%2

PATH=%netDir%;%PATH%


set "files=Program.cs MainWindow.cs Track.cs Device.cs Scene.cs Shader.cs Mesh.cs Matrix.cs"
pushd source
call csc.exe %files% -lib:%opentkpath% -r:OpenTK.dll,System.Drawing.dll -out:..\build\demotest.exe
popd
IF NOT EXIST build\OpenTK.dll copy %opentkpath%\OpenTK.dll build\