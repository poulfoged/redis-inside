@echo off
cd %1
cd ..
del *.nupkg
dotnet pack -c Release -o %cd% source\RedisInside\RedisInside.csproj
