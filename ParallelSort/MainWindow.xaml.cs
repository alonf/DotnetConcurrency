using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ParallelSort
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Line[] _lines = new Line[0];
        readonly Random _rnd = new Random();
        private bool _inSort;

        public MainWindow()
        {
            InitializeComponent();

        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            Play();
        }

        private static bool UpdateColor(ref double color, ref double dColor)
        {
            var newColor = color + dColor;
            if (newColor >= 240 || newColor <= 30)
            {
                dColor = -dColor / 1.3;
                color += dColor;
                return true;
            }
            color = newColor;
            return false;
        }

        private void Play()
        {
            if (_inSort)
                return;

            _inSort = true;

            if (_lines != null)
            {
                foreach (var line in _lines)
                {
                    _mainCanvas.Children.Remove(line);
                }
            }

            _lines = new Line[(int)_mainCanvas.ActualWidth];

            double dr = 0.5, db = 0.9, dg = 0.7;
            double red = _rnd.Next(31, 220), blue = _rnd.Next(31, 220), green = _rnd.Next(31, 220);


            for (int x = 0; x < _lines.Length; ++x)
            {
                var y = ((_mainCanvas.ActualHeight - 10) / _mainCanvas.ActualWidth) * x + 10;
                var line = new Line
                {
                    Stroke = new SolidColorBrush(Color.FromRgb((byte)red, (byte)green, (byte)blue)),
                    StrokeThickness = 2,
                    X1 = x,
                    X2 = x,
                    Y1 = 10,
                    Y2 = y
                };

                if (UpdateColor(ref red, ref dr))
                    if (UpdateColor(ref green, ref dg))
                        UpdateColor(ref blue, ref db);

                _mainCanvas.Children.Add(line);
                _lines[x] = line;
                Canvas.SetBottom(line, 10);
            }

            for (int x = 0; x < _lines.Length - 1; ++x)
            {
                var randomLocation = _rnd.Next(x + 1, _lines.Length);
                SwapLinePositions(x, randomLocation, TimeSpan.Zero);
            }
            Task.Run(new Action(Sort));
        }

        private int Partition(int left, int right, int pivotIndex)
        {
            var pivotValue = _lines[pivotIndex].SafeGetY2();
            SwapLinePositions(pivotIndex, right, TimeSpan.FromMilliseconds(5));
            var storeIndex = left;

            for (int i = left; i < right; i++)
            {
                if (_lines[i].SafeGetY2() < pivotValue)
                {
                    SwapLinePositions(i, storeIndex, TimeSpan.FromMilliseconds(5));
                    storeIndex++;
                }
            }
            SwapLinePositions(storeIndex, right, TimeSpan.FromMilliseconds(5));
            return storeIndex;
        }

        private void SwapLinePositions(int source, int target, TimeSpan delay)
        {
            _lines[source].SafeSetX1(target);
            _lines[source].SafeSetX2(target);
            _lines[target].SafeSetX1(source);
            _lines[target].SafeSetX2(source);
            var tempLine = _lines[source];
            _lines[source] = _lines[target];
            _lines[target] = tempLine;
            if (delay != TimeSpan.Zero)
                Thread.Sleep(delay);
        }

        private void Sort()
        {
            SerialQuickSort(0, _lines.Length - 1);
            //QuickSort(0, _lines.Length - 1);
            _inSort = false;
        }

        private void SerialQuickSort(int left, int right)
        {
            int pivotIndex = (left + right) / 2;
            if (left >= right)
                return;

            var pivotNewIndex = Partition(left, right, pivotIndex);
            SerialQuickSort(left, pivotNewIndex - 1);
            SerialQuickSort(pivotNewIndex + 1, right);
        }

        // ReSharper disable UnusedMember.Local
        private void QuickSort(int left, int right)
        // ReSharper restore UnusedMember.Local
        {
            int pivotIndex = (left + right) / 2;
            if (left >= right)
                return;

            var pivotNewIndex = Partition(left, right, pivotIndex);
            var leftTask = Task.Run(() => QuickSort(left, pivotNewIndex - 1));
            var rightTask = Task.Run(() => QuickSort(pivotNewIndex + 1, right));
            Task.WaitAll(leftTask, rightTask);
        }
    }

    public static class ThreadSafeLineExtension
    {
        private static double GetSafeLineValue(Line line, Func<Line, double> func)
        {
            return (double)line.Dispatcher.Invoke(func, line);
        }

        private static void SetSafeLineValue(Line line, Action<Line, double> func, double value)
        {
            line.Dispatcher.Invoke(func, line, value);
        }

        public static double SafeGetX1(this Line line)
        {
            return GetSafeLineValue(line, l => l.X1);
        }

        public static void SafeSetX1(this Line line, double value)
        {
            SetSafeLineValue(line, (l, v) => l.X1 = v, value);
        }

        public static double SafeGetX2(this Line line)
        {
            return GetSafeLineValue(line, l => l.X2);
        }

        public static void SafeSetX2(this Line line, double value)
        {
            SetSafeLineValue(line, (l, v) => l.X2 = v, value);
        }

        public static double SafeGetY1(this Line line)
        {
            return GetSafeLineValue(line, l => l.Y1);
        }

        public static void SafeSetY1(this Line line, double value)
        {
            SetSafeLineValue(line, (l, v) => l.Y1 = v, value);
        }

        public static double SafeGetY2(this Line line)
        {
            return GetSafeLineValue(line, l => l.Y2);
        }

        public static void SafeSetY2(this Line line, double value)
        {
            SetSafeLineValue(line, (l, v) => l.Y2 = v, value);
        }

    }
}
