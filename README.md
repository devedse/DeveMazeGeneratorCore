# DeveMazeGeneratorCore
This is the new version of my maze generator, now made with .NET Core.

## Build status

| Travis (Linux/Osx build) | AppVeyor (Windows build) |
| ------------------------ |:------------------------:|
| [![Build Status](https://travis-ci.org/devedse/DeveMazeGeneratorCore.svg?branch=master)](https://travis-ci.org/devedse/DeveMazeGeneratorCore) | [![Build status](https://ci.appveyor.com/api/projects/status/ainctv2tnoxg2t86?svg=true)](https://ci.appveyor.com/project/devedse/devemazegeneratorcore) |

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
* DeveMazeGenerator.x.x.x-CI.nupkg (Nuget package of library)
* DeveMazeGenerator.x.x.x-CI.symbols.nupkg (Nuget package of symbols for library)