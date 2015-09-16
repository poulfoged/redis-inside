@echo off
cd %1
cd ..
del *.nupkg
build\tools\nuget.exe pack source\RedisInside\RedisInside.csproj
