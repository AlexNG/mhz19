using System;
using System.IO;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using Microsoft.Extensions.Configuration;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveUI;
using DateTimeAxis = OxyPlot.Axes.DateTimeAxis;

namespace mhz19
{
    using Mhz.Core;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Core.Config = new Config();
            new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                .AddJsonFile("appsettings.json")
                .Build()
                .GetSection("ApplicationConfiguration")
                .Bind(Core.Config);
            var model = new MainWindowViewModel(Core.Config);
            DataContext = model;
            if (Core.Config.AutoConnect)
            {
                model.Start.Execute(null);
            }
        }
    }

    public class MainWindowViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<int> _ppm;

        private DataWindow dataWindow;

        public int Ppm => _ppm.Value;

        public PlotModel Plot { get; }

        public MainWindowViewModel(Config config)
        {
            dataWindow = new DataWindow(config.StabilizationInterval / Config.QueryInterval, config.WarningLevel, config.StabilizationInterval)
            {
                Event = (sender, eventArgs) => MessageBox.Show($"{eventArgs} > {config.WarningLevel}", "CO2", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            };
            Plot = new PlotModel { Title = "График концентрации углекислого газа" };

            Plot.Axes.Add(new DateTimeAxis { Minimum = DateTimeAxis.ToDouble(DateTime.Now),
                                             Title = "Время",
                                             Maximum = DateTimeAxis.ToDouble(DateTime.Now.AddMinutes(5))});
            Plot.Axes.Add(new LinearAxis { Title = "CO2, ppm", Minimum = 200, Maximum = 2000});

            Plot.Series.Add(new LineSeries { ItemsSource = Items });

            Start = ReactiveCommand.CreateAsyncObservable(_ => Core.Instance.GetPpm().TakeUntil(Stop));
            Start.ThrownExceptions.Subscribe(e => Console.WriteLine(e.Message));

            Stop = ReactiveCommand.Create(Start.IsExecuting);

            _ppm = Start.ToProperty(this, m => m.Ppm);

            this.WhenAnyValue(m => m.Ppm).Where(v => v > 0).Subscribe(newValue =>
            {
                Items.Add(DateTimeAxis.CreateDataPoint(DateTime.Now, newValue));
                Plot.InvalidatePlot(true);
                dataWindow.Add(newValue);
            });
        }

        private ReactiveList<DataPoint> Items { get; } = new ReactiveList<DataPoint>();

        public ReactiveCommand<object> Stop { get; }

        public ReactiveCommand<int> Start { get; }
    }
}
