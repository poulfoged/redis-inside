@echo off
del *.nupkg
dotnet pack -c Release -o %cd% source\RedisInside\RedisInside.csproj
dotnet nuget push *.nupkg
