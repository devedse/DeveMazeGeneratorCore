# Stage 1
FROM microsoft/dotnet:2.0.5-sdk-2.1.4-stretch AS builder
WORKDIR /source

# caches restore result by copying csproj file separately
COPY /NuGet.config /source/
COPY /src/DeveMazeGenerator/*.csproj /source/src/DeveMazeGenerator/
COPY /src/DeveMazeGeneratorConsole/*.csproj /source/src/DeveMazeGeneratorConsole/
COPY /src/DeveMazeGeneratorWeb/*.csproj /source/src/DeveMazeGeneratorWeb/
COPY /DeveMazeGenerator.sln /source/
RUN ls
RUN dotnet restore

# copies the rest of your code
COPY . .
RUN dotnet publish ./src/DeveMazeGeneratorWeb/DeveMazeGeneratorWeb.csproj --output /app/ --configuration Release

# Stage 2
FROM microsoft/aspnetcore:2.0.5-stretch
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "DeveMazeGeneratorWeb.dll"]



#FROM microsoft/aspnetcore:1.1.2-jessie
#ADD Publish /DeveMazeGeneratorCoreWebPublish
#EXPOSE 80
#WORKDIR "/DeveMazeGeneratorCoreWebPublish"
#CMD ["dotnet", "DeveMazeGeneratorWeb.dll", "--server.urls=http://*:80"]