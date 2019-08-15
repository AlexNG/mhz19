using System;

namespace Mhz.Core
{
    public interface ICore
    {
        IConfig Config { get; set; }

        int SendCommandAndGetPpm();

        int Ppm { get; }
    }
}