using System;

namespace mhz19
{
    /// <summary>
    /// With cyclic storage
    /// </summary>
    public class DataWindow
    {
        private double[] data;
        private double sum;
        private int pointer = -1;
        private int actualWidth;
        private readonly int warningLevel;
        private readonly int stabilizationInterval;
        private DateTime lastNotification;

        public double Avg { get; private set; }

        public EventHandler<double> Event;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="width"></param>
            /// <param name="warningLevel"></param>
            /// <param name="stabilizationInterval">Should somehow depend on window width, but should be much bigger</param>
        public DataWindow(int width, int warningLevel, int stabilizationInterval)
        {
            data = new double[width];
            this.warningLevel = warningLevel;
            this.stabilizationInterval = stabilizationInterval;
        }

        public void Add(double value)
        {
            pointer = pointer >= data.Length - 1 ? 0 : pointer + 1;
            if (actualWidth < data.Length)
            {
                actualWidth++;
            }
            double curr = data[pointer];
            data[pointer] = value;
            sum = sum - curr + value;
            Avg = sum / actualWidth;
            Check();
        }

        private void Check()
        {
            bool bigger = Avg > warningLevel;
            if (bigger && (DateTime.Now - lastNotification).TotalSeconds > stabilizationInterval)
            {
                Event?.Invoke(this, Avg);
                lastNotification = DateTime.Now;
            }
        }
    }
}