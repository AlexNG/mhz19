using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Mhz.Core
{
    public class Config : IConfig
    {
        public Config()
        {
            Load();
        }

        public void Load()
        {
            new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                .AddJsonFile("appsettings.json")
                .Build()
                .GetSection("ApplicationConfiguration")
                .Bind(this);
        }

        public bool AutoConnect { get; set; } = true;

        public int ComPortNumber { get; set; }

        /// <summary>
        /// Seconds
        /// </summary>
        public int QueryInterval => 10;

        public int StabilizationInterval { get; set; } = 300;

        /// <inheritdoc />
        public int WarningLevel { get; set; } = 1000;
    }
}
