FROM microsoft/aspnetcore:2.0.0-preview1-jessie
ADD DeveMazeGeneratorCoreWebPublish /DeveMazeGeneratorCoreWebPublish
EXPOSE 80
WORKDIR "/DeveMazeGeneratorCoreWebPublish"
CMD ["dotnet", "DeveMazeGeneratorWeb.dll", "--server.urls=http://*:80"]