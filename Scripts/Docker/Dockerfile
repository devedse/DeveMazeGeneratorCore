FROM microsoft/aspnetcore:1.1.2-jessie
ADD DeveMazeGeneratorCoreWebPublish /DeveMazeGeneratorCoreWebPublish
EXPOSE 80
WORKDIR "/DeveMazeGeneratorCoreWebPublish"
CMD ["dotnet", "DeveMazeGeneratorWeb.dll", "--server.urls=http://*:80"]