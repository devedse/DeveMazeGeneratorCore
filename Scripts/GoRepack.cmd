call "%~dp0Cleanup.cmd"

rem xcopy "%~dp0..\src\DeveMazeGenerator\bin\Release\netstandard1.0" "%~dp0Output\" /e /y

robocopy "%~dp0..\src\DeveMazeGenerator\bin\Release\netstandard1.0" "%~dp0Output" /E /xf *.pdb

"%~dp0\7z_x64_1602\7za.exe" a -mm=Deflate -mfb=258 -mpass=15 "%~dp0DeveMazeGeneratorCore.zip" "%~dp0Output\*"
"%~dp0\7z_x64_1602\7za.exe" a -t7z -m0=LZMA2 -mmt=on -mx9 -md=1536m -mfb=273 -ms=on -mqs=on -sccUTF-8 "%~dp0DeveMazeGeneratorCore.7z" "%~dp0Output\*"