FROM microsoft/dotnet:latest
ADD DeveMazeGeneratorCoreWebPublish /home/DeveMazeGeneratorCoreWebPublish
CMD ["dotnet", "/home/DeveMazeGeneratorCoreWebPublish/DeveMazeGeneratorWeb.dll", "--server.urls=http://*:80"]