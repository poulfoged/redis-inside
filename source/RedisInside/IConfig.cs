using System;

namespace RedisInside
{
    public interface IConfig
    {
        IConfig Port(int portNumber);
        IConfig LogTo(Action<string> logFunction);
    }
}