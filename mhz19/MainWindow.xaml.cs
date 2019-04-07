using System;
using System.IO.Ports;
using System.Reactive.Linq;
using System.Threading;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveUI;
using DateTimeAxis = OxyPlot.Axes.DateTimeAxis;

namespace mhz19
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }

    public class MainWindowViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<int> _ppm;

        public int Ppm => _ppm.Value;

        public PlotModel Plot { get; }

        public MainWindowViewModel()
        {
            Plot = new PlotModel { Title = "График концентрации углекислого газа" };

            Plot.Axes.Add(new DateTimeAxis { Minimum = DateTimeAxis.ToDouble(DateTime.Now),
                                             Title = "Время",
                                             Maximum = DateTimeAxis.ToDouble(DateTime.Now.AddMinutes(5))});
            Plot.Axes.Add(new LinearAxis { Title = "CO2, ppm", Minimum = 200, Maximum = 1000});

            Plot.Series.Add(new LineSeries { ItemsSource = Items });

            Start = ReactiveCommand.CreateAsyncObservable(_ => Core.Instance.GetPpm().TakeUntil(Stop));
            Start.ThrownExceptions.Subscribe(e => Console.WriteLine(e.Message));

            Stop = ReactiveCommand.Create(Start.IsExecuting);

            _ppm = Start.ToProperty(this, m => m.Ppm);

            this.WhenAnyValue(m => m.Ppm).Where(v => v > 0).Subscribe(newValue =>
            {
                Items.Add(DateTimeAxis.CreateDataPoint(DateTime.Now, newValue));
                Plot.InvalidatePlot(true);
            });
        }

        private ReactiveList<DataPoint> Items { get; } = new ReactiveList<DataPoint>();

        public ReactiveCommand<object> Stop { get; }

        public ReactiveCommand<int> Start { get; }
    }

    public class Core : IDisposable
    {
        private readonly byte[] _command = { 0xff, 0x01, 0x86, 0x00, 0x00, 0x00, 0x00, 0x00, 0x79 };
        private static Core _instance;
        private readonly SerialPort _port;

        private Core()
        {
            _port = new SerialPort
            {
                BaudRate = 9600,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None,
                PortName = "COM3"
            };
        }

        public IObservable<int> GetPpm()
        {
            if (!_port.IsOpen)
                _port.Open();

            return Observable.Interval(TimeSpan.FromSeconds(10)).StartWith(0).Select(_ => SendCommandAndGetPpm());
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
