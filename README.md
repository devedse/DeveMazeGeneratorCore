# DeveMazeGeneratorCore

[![Join the chat at https://gitter.im/DeveMazeGeneratorCore/Lobby](https://badges.gitter.im/DeveMazeGeneratorCore/Lobby.svg)](https://gitter.im/DeveMazeGeneratorCore/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This is the new version of my maze generator, now made with .NET Core.

## Build status

| Travis (Linux/Osx build) | AppVeyor (Windows build) | Github Actions (Windows and Linux build) | Github Actions (Docker build) |
|:------------------------:|:------------------------:|:----------------------------------------:|:-----------------------------:|
| [![Build Status](https://travis-ci.org/devedse/DeveMazeGeneratorCore.svg?branch=master)](https://travis-ci.org/devedse/DeveMazeGeneratorCore) | [![Build status](https://ci.appveyor.com/api/projects/status/ainctv2tnoxg2t86?svg=true)](https://ci.appveyor.com/project/devedse/devemazegeneratorcore) | [![.NET Core](https://github.com/devedse/DeveMazeGeneratorCore/workflows/.NET%20Core/badge.svg)](https://github.com/devedse/DeveMazeGeneratorCore/actions?query=workflow%3A%22.NET+Core%22) | [![Docker](https://github.com/devedse/DeveMazeGeneratorCore/workflows/docker/badge.svg)](https://github.com/devedse/DeveMazeGeneratorCore/actions?query=workflow%3Adocker) |

## DockerHub

| Docker Hub |
|:----------:|
| [![Docker pulls](https://img.shields.io/docker/v/devedse/devemazegeneratorcore)](https://hub.docker.com/r/devedse/devemazegeneratorcore/) |

## Intellicode

|  Github Actions (Intellicode) |
|:-----------------------------:|
| [![Main IntelliCode Workflow](https://github.com/devedse/DeveMazeGeneratorCore/workflows/Main%20IntelliCode%20Workflow/badge.svg)](https://github.com/devedse/DeveMazeGeneratorCore/actions?query=workflow%3A%22Main+IntelliCode+Workflow%22) |

## Code Coverage Status

| CodeCov |
|:-------:|
| [![codecov](https://codecov.io/gh/devedse/DeveMazeGeneratorCore/branch/master/graph/badge.svg)](https://codecov.io/gh/devedse/DeveMazeGeneratorCore) |

## Code Quality Status

| SonarQube |
|:---------:|
| [![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=DeveMazeGeneratorCore&metric=alert_status)](https://sonarcloud.io/dashboard?id=DeveMazeGeneratorCore) |

## Package

| NuGet |
|:-----:|
| [![NuGet](https://img.shields.io/nuget/v/DeveMazeGeneratorCore.svg)](https://www.nuget.org/packages/DeveMazeGeneratorCore/) |

## Deployment status

(If an image of a Maze is shown below, the deployment is working)

| Azure Web Deployment | Azure Docker Deployment |
|:--------------------:|:-----------------------:|
| [![Azure web deployment down :(](http://devemazegeneratorcoreweb.azurewebsites.net/api/mazes/MazePath/192/64)](http://devemazegeneratorcoreweb.azurewebsites.net/api/mazes/MazePath/192/64) | [![Docker deployment down :(](http://devemazegeneratorcoredocker.azurewebsites.net/api/mazes/MazePath/192/64)](http://devemazegeneratorcoredocker.azurewebsites.net/api/mazes/MazePath/192/64) |

## Build and Deployment details

### Travis

The Travis build will also run publish and then create a docker image which is automatically published to here:
https://hub.docker.com/r/devedse/devemazegeneratorcore/

Azure will then pick up the docker image and automatically deploy it using the Web App On Linux (preview) to:
http://devemazegeneratorcoredocker.azurewebsites.net/api/mazes/MazePath/512/512

Azure will also do a seperate deployment/build when a push to git has occured:
http://devemazegeneratorcoreweb.azurewebsites.net/api/mazes/MazePath/512/512

### AppVeyor:

AppVeyor will create a number of build artefacts which are added as releases on Github so they can be downloaded:
* DeveMazeGeneratorCore.7z (Build output as 7z)
* DeveMazeGeneratorCore.zip (Build output as zip)
* DeveMazeGeneratorCore.x.x.x.nupkg (Nuget package of library)
* DeveMazeGeneratorCore.x.x.x.symbols.nupkg (Nuget package of symbols for library)

## Maze generator details

As of the latest version it is now also possible to generate mazes the size on 2^30 * 2^30 dynamically with a path.

Use the following url as an example:
http://devemazegeneratorcoredocker.azurewebsites.net/api/mazes/MazeDynamicPathSeedPart/1337/1073741824/1073741824/0/0/512/512
http://devemazegeneratorcoreweb.azurewebsites.net/api/mazes/MazeDynamicPathSeedPart/1337/1073741824/1073741824/0/0/512/512
