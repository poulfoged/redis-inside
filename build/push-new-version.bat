@echo off
del *.nupkg
tools\nuget pack ..\source\RedisInside\RedisInside.csproj
tools\nuget push *.nupkg
