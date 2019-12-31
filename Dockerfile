# Stage 1
FROM microsoft/dotnet:2.2-sdk AS builder
WORKDIR /source

# caches restore result by copying csproj file separately
COPY /NuGet.config /source/
COPY /src/DeveMazeGenerator/*.csproj /source/src/DeveMazeGenerator/
COPY /src/DeveMazeGeneratorConsole/*.csproj /source/src/DeveMazeGeneratorConsole/
COPY /src/DeveMazeGeneratorWeb/*.csproj /source/src/DeveMazeGeneratorWeb/
COPY /test/DeveMazeGenerator.Tests/*.csproj /source/test/DeveMazeGenerator.Tests/
COPY /DeveMazeGenerator.sln /source/
RUN ls
RUN dotnet restore

# copies the rest of your code
COPY . .
RUN dotnet build --configuration Release
RUN dotnet test --configuration Release ./test/DeveMazeGenerator.Tests/DeveMazeGenerator.Tests.csproj
RUN dotnet publish ./src/DeveMazeGeneratorWeb/DeveMazeGeneratorWeb.csproj --output /app/ --configuration Release

# Stage 2
FROM microsoft/dotnet:2.2-aspnetcore-runtime-alpine
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "DeveMazeGeneratorWeb.dll"]