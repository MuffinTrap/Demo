@echo off 
@CLEAR

rem Builds demoparty

SET "netDir=C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\Roslyn\"

PATH=%netDir%;%PATH%

set "opentkpath=C:\Dev\Monogame\Demo\ConsoleApp1\packages\OpenTK.3.0.0-pre\lib\net20"
set "files=Program.cs MainWindow.cs Track.cs Device.cs Scene.cs Shader.cs Mesh.cs Matrix.cs"
call csc.exe %files% -lib:%opentkpath% -r:OpenTK.dll,System.Drawing.dll -out:build\demotest.exe
IF NOT EXIST build\OpenTK.dll copy %opentkpath%\OpenTK.dll build\