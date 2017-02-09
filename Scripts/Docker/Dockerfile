FROM microsoft/dotnet:latest
ADD DeveMazeGeneratorCoreWebPublish /usr/local/bin/DeveMazeGeneratorCoreWebPublish
CMD ["dotnet", "/usr/local/bin/DeveMazeGeneratorCoreWebPublish/DeveMazeGeneratorWeb.dll", "--server.urls=http://*:80"]