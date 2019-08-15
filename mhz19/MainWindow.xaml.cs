using System;
using System.Reactive.Linq;
using System.Windows;
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
        private IConfig Config { get; }
        private ICore Core { get; }

        public MainWindow()
        {
            InitializeComponent();
            Config = new Config();
            Core = new Core(Config);
            var model = new MainWindowViewModel(Config, Core);
            DataContext = model;
            if (Config.AutoConnect)
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

        public MainWindowViewModel(IConfig config, ICore core)
        {
            dataWindow = new DataWindow(config.StabilizationInterval / config.QueryInterval, config.WarningLevel, config.StabilizationInterval)
            {
                Event = (sender, eventArgs) => MessageBox.Show($"{eventArgs} > {config.WarningLevel}", "CO2", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            };
            Plot = new PlotModel { Title = "График концентрации углекислого газа" };

            Plot.Axes.Add(new DateTimeAxis { Minimum = DateTimeAxis.ToDouble(DateTime.Now),
                                             Title = "Время",
                                             Maximum = DateTimeAxis.ToDouble(DateTime.Now.AddMinutes(5))});
            Plot.Axes.Add(new LinearAxis { Title = "CO2, ppm", Minimum = 200, Maximum = 2000});

            Plot.Series.Add(new LineSeries { ItemsSource = Items });

            Start = ReactiveCommand.CreateAsyncObservable(_ =>
                Observable.Interval(TimeSpan.FromSeconds(config.QueryInterval)).StartWith(0).Select(__ => core.SendCommandAndGetPpm())
                .TakeUntil(Stop));
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
