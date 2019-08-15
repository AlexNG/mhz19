namespace Mhz.Core
{
    public interface IConfig
    {
        bool AutoConnect { get; set; }
        int QueryInterval { get; }
        int ComPortNumber { get; set; }

        /// <summary>
        /// Seconds
        /// </summary>
        int StabilizationInterval { get; set; }

        /// <summary>
        /// PPM
        /// </summary>
        int WarningLevel { get; set; }
    }
}
