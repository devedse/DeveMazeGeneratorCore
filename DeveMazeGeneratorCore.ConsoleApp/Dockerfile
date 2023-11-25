#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DeveMazeGeneratorCore.ConsoleApp/DeveMazeGeneratorCore.ConsoleApp.csproj", "DeveMazeGeneratorCore.ConsoleApp/"]
COPY ["DeveMazeGeneratorCore/DeveMazeGeneratorCore.csproj", "DeveMazeGeneratorCore/"]
RUN dotnet restore "DeveMazeGeneratorCore.ConsoleApp/DeveMazeGeneratorCore.ConsoleApp.csproj"
COPY . .
WORKDIR "/src/DeveMazeGeneratorCore.ConsoleApp"
RUN dotnet build "DeveMazeGeneratorCore.ConsoleApp.csproj" -c Release -o /app/build

FROM build AS publish
ARG BUILD_VERSION
ARG VER=${BUILD_VERSION:-1.0.0}
RUN dotnet publish "DeveMazeGeneratorCore.ConsoleApp.csproj" -c Release -o /app/publish /p:Version=$VER

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DeveMazeGenerator.ConsoleApp.dll"]
