![](https://raw.githubusercontent.com/poulfoged/redis-inside/master/icon.png) &nbsp; ![](https://ci.appveyor.com/api/projects/status/5m2rpq1gokv0geu3?svg=true) &nbsp; ![](http://img.shields.io/nuget/v/redis-inside.svg?style=flat)
#Redis Inside  

Run integration tests against Redis without having to start/install an instance.

Redis inside works by extracting the Redis executable to a temporary location and executing it. Internally it uses [Redis for windows](https://github.com/MSOpenTech/redis) ported by [MS Open Tech](https://msopentech.com/opentech-projects/redis).


## How to
Launch a Redis instance from just by creating a new instance of Redis. After that the node name and port can be acceessed from the node-property:

```c#
using (var redis = new Redis())
{
    // connect to redis.Endpoint here
}

```

Each instance will run on a random port so you can even run multiple instances:

```c#
using (var redis1 = new Redis())
using (var redis2 = new Redis())
{
    // connect to two nodes here
}

```
## Install

Simply add the Nuget package:

`PM> Install-Package redis-inside`

## Requirements

You'll need .NET Framework 4.5.1 or dotnet core 2.0 or later on 64 bit Windows to use the precompiled binaries.

## License

Redis Inside is licensed under the two clause BSD license. Redis is copyrighted by Salvatore Sanfilippo and Pieter Noordhuis and released under the [terms of the three clause BSD license](http://redis.io/topics/license).
