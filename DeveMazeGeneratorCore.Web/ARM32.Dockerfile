#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.8-buster-slim-arm32v7 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.402-buster-arm32v7 AS build
WORKDIR /src
COPY ["DeveMazeGeneratorCore.Web/DeveMazeGeneratorCore.Web.csproj", "DeveMazeGeneratorCore.Web/"]
COPY ["DeveMazeGeneratorCore/DeveMazeGeneratorCore.csproj", "DeveMazeGeneratorCore/"]
RUN dotnet restore "DeveMazeGeneratorCore.Web/DeveMazeGeneratorCore.Web.csproj"
COPY . .
WORKDIR "/src/DeveMazeGeneratorCore.Web"
RUN dotnet build "DeveMazeGeneratorCore.Web.csproj" -c Release -o /app/build

FROM build AS publish
ARG BUILD_VERSION
ARG VER=${BUILD_VERSION:-1.0.0}
RUN dotnet publish "DeveMazeGeneratorCore.Web.csproj" -c Release -o /app/publish /p:Version=$VER

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DeveMazeGeneratorCore.Web.dll"]
