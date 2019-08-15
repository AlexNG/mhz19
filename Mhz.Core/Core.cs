using System;
using System.IO.Ports;
using System.Threading;

namespace Mhz.Core
{
    public class Core : IDisposable, ICore
    {
        private readonly byte[] _command = { 0xff, 0x01, 0x86, 0x00, 0x00, 0x00, 0x00, 0x00, 0x79 };

        private readonly SerialPort _port;

        public IConfig Config { get; set; }

        volatile int ppm;

        public int Ppm => ppm;

        public Core(IConfig config)
        {
            Config = config;
            _port = new SerialPort { BaudRate = 9600, DataBits = 8, StopBits = StopBits.One, Parity = Parity.None, PortName = "COM" + Config.ComPortNumber };
        }

        public int SendCommandAndGetPpm()
        {
            if (!_port.IsOpen)
                _port.Open();

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

            this.ppm = ppm;
            return ppm;
        }

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