#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["NuGet.config", "/"]
COPY ["src/DeveMazeGeneratorWeb/DeveMazeGeneratorWeb.csproj", "src/DeveMazeGeneratorWeb/"]
COPY ["src/DeveMazeGenerator/DeveMazeGenerator.csproj", "src/DeveMazeGenerator/"]
RUN dotnet restore "src/DeveMazeGeneratorWeb/DeveMazeGeneratorWeb.csproj"
COPY . .
WORKDIR "/src/src/DeveMazeGeneratorWeb"
RUN dotnet build "DeveMazeGeneratorWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DeveMazeGeneratorWeb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD dotnet DeveMazeGeneratorWeb.dll --urls=http://+:$PORT/