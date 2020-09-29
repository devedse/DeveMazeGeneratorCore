#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["NuGet.config", "/"]
COPY ["DeveMazeGenerator.Web/DeveMazeGenerator.Web.csproj", "DeveMazeGenerator.Web/"]
COPY ["DeveMazeGenerator/DeveMazeGenerator.csproj", "DeveMazeGenerator/"]
RUN dotnet restore "DeveMazeGenerator.Web/DeveMazeGenerator.Web.csproj"
COPY . .
WORKDIR "/src/DeveMazeGenerator.Web"
RUN dotnet build "DeveMazeGenerator.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DeveMazeGenerator.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD dotnet DeveMazeGenerator.Web.dll --urls=http://+:$PORT/