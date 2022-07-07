#FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim-amd64 AS build
##Also install dotnet 3.1
##RUN curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin -Channel 3.1 -InstallDir /usr/share/dotnet
##RUN apt update && apt install libfreetype6 -y

FROM nginx:stable-alpine AS base
COPY ./DeveMazeGeneratorCore.MonoGame.Blazor/nginxconfig/nginx.conf /etc/nginx/nginx.conf
#FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DeveMazeGeneratorCore.MonoGame.Blazor/DeveMazeGeneratorCore.MonoGame.Blazor.csproj", "DeveMazeGeneratorCore.MonoGame.Blazor/"]
RUN dotnet restore "DeveMazeGeneratorCore.MonoGame.Blazor/DeveMazeGeneratorCore.MonoGame.Blazor.csproj"
COPY . .
WORKDIR "/src/DeveMazeGeneratorCore.MonoGame.Blazor"
RUN dotnet build "DeveMazeGeneratorCore.MonoGame.Blazor.csproj" -c Release -o /app/build

FROM build AS publish
ARG BUILD_VERSION
ARG VER=${BUILD_VERSION:-1.0.0}
RUN dotnet publish "DeveMazeGeneratorCore.MonoGame.Blazor.csproj" -c Release -o /app/publish /p:Version=$VER /p:UseAppHost=false

FROM base AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
CMD ["nginx", "-g", "daemon off;"]
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /root/.nuget/packages/microsoft.aspnetcore.components.webassembly.devserver/6.0.2/tools .
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "blazor-devserver.dll", "--applicationpath", "DeveMazeGeneratorCore.MonoGame.Blazor.dll"]