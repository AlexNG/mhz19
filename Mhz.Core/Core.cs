using System;
using System.Threading;

namespace Mhz.Core
{
    using System.IO.Ports;
    using System.Reactive.Linq;

    public class Core : IDisposable
    {
        private readonly byte[] _command = { 0xff, 0x01, 0x86, 0x00, 0x00, 0x00, 0x00, 0x00, 0x79 };

        private static Core _instance;

        private readonly SerialPort _port;

        public static Config Config { get; set; }

        public volatile int Ppm;

        private Core()
        {
            _port = new SerialPort { BaudRate = 9600, DataBits = 8, StopBits = StopBits.One, Parity = Parity.None, PortName = "COM" + Config.ComPortNumber };
        }

        public IObservable<int> GetPpm()
        {
            if (!_port.IsOpen)
                _port.Open();

            return Observable.Interval(TimeSpan.FromSeconds(Config.QueryInterval)).StartWith(0).Select(_ => SendCommandAndGetPpm());
        }

        private int SendCommandAndGetPpm()
        {
            if (!_port.IsOpen)
                return -1;

            var ppm = 0;
            _port.Write(_command, 0, 9);

            Thread.Sleep(100);
            if (_port.BytesToRead != 9)
                return -1;

            var buffer = new byte[9];
            _port.Read(buffer, 0, 9);

            byte crc = 0;
            for (var i = 1; i < 8; i++)
                crc += buffer[i];

            crc = (byte)(0xff ^ crc);
            crc += 1;

            if (buffer[0] == 0xff && buffer[1] == 0x86 && buffer[8] == crc)
            {
                ppm = buffer[2] * 256 + buffer[3];
            }

            Ppm = ppm;
            return ppm;
        }

        public static Core Instance => _instance ?? (_instance = new Core());

        public void Dispose()
        {
            if (_port != null)
            {
                _port.Close();
                _port.Dispose();
            }
        }
    }
}