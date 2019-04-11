namespace Mhz.Core
{
    public class Config
    {
        public Config()
        {
            this.AutoConnect = true;
        }

        public bool AutoConnect { get; set; }

        public int ComPortNumber { get; set; }

        public static readonly int QueryInterval = 10; // seconds

        public int StabilizationInterval { get; set; } = 300; // seconds

        public int WarningLevel { get; set; } = 1000; // PPM
    }
}
