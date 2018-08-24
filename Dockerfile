# Stage 1
FROM microsoft/dotnet:2.1-sdk-alpine AS builder
WORKDIR /source

# caches restore result by copying csproj file separately
COPY /NuGet.config /source/
COPY /src/DeveMazeGenerator/*.csproj /source/src/DeveMazeGenerator/
COPY /src/DeveMazeGeneratorConsole/*.csproj /source/src/DeveMazeGeneratorConsole/
COPY /src/DeveMazeGeneratorWeb/*.csproj /source/src/DeveMazeGeneratorWeb/
copy /test/DeveMazeGenerator.Tests/*.csproj /source/test/DeveMazeGenerator.Tests/
COPY /DeveMazeGenerator.sln /source/
RUN ls
RUN dotnet restore

# copies the rest of your code
COPY . .
RUN dotnet build --configuration Release
RUN dotnet test --configuration Release ./test/DeveMazeGenerator.Tests/DeveMazeGenerator.Tests.csproj
RUN dotnet publish ./src/DeveMazeGeneratorWeb/DeveMazeGeneratorWeb.csproj --output /app/ --configuration Release

# Stage 2
FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "DeveMazeGeneratorWeb.dll"]



#FROM microsoft/aspnetcore:1.1.2-jessie
#ADD Publish /DeveMazeGeneratorCoreWebPublish
#EXPOSE 80
#WORKDIR "/DeveMazeGeneratorCoreWebPublish"
#CMD ["dotnet", "DeveMazeGeneratorWeb.dll", "--server.urls=http://*:80"]