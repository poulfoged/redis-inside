mkdir temp
cd temp
..\tools\nuget.exe install redis-64

$redisDir = Get-ChildItem -Recurse $directory | Where-Object { $_.PSIsContainer -and `
    $_.Name.StartsWith("Redis") } | Select-Object -First 1

Remove-Item ..\..\source\RedisInside\Executables\redis-server.exe -Force

..\tools\upx --brute $redisDir\redis-server.exe -o..\..\source\RedisInside\Executables\redis-server.exe

cd ..
Remove-Item temp -Force -Recurse