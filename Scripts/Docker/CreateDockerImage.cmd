robocopy "%~dp0..\..\src\DeveMazeGeneratorWeb\bin\Release\netcoreapp1.0\publish" "%~dp0DeveMazeGeneratorCoreWebPublish" /MIR
docker build -t devedse/devemazegeneratorcore .
pause