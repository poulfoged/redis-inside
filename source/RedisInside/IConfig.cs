using System;

namespace ElasticsearchInside
{
    public interface IConfig
    {
        IConfig Port(int portNumber);
        IConfig LogTo(Action<string> logFunction);
    }
}