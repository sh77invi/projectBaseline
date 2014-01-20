using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChartingUtility.LineCharts;
using System.Windows.Controls;
using System.Windows;

namespace Dashboard
{
    class LineChart
    {
        public ChartStyleGridlines CS { get; set; }
        public DataSeries DS { get; set; }
        public DataCollection DC { get; set; }
        public TimeSpan YMin { get; set; }
        public TimeSpan YMax { get; set; }
        public string YLabel { get; set; }
        public bool IsTimeDuration { get; set; }

        private Grid m_topGrid;
        private Grid m_chartGrid;
        private TextBlock m_tbTitle;
        private TextBlock m_tbXlabel;
        private TextBlock m_tbYlabel;
        private Canvas m_textCanvas;
        private Canvas m_chartCanvas;
        private List<Tuple<long, long>> m_data;

        public LineChart(Grid grid, IEnumerable<Tuple<DateTime, TimeSpan>> data)
        {
            IsTimeDuration = false;

            m_topGrid = grid;
            m_data = data.Select(d => new Tuple<long,long>(d.Item1.Ticks, d.Item2.Ticks)).ToList();

            m_topGrid.Children.Clear();
            m_topGrid.ColumnDefinitions.Clear();
            m_topGrid.RowDefinitions.Clear();

            m_topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            m_topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            m_topGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            m_topGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            m_topGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            m_tbTitle = GetFormattedTextBlock();
            m_tbTitle.HorizontalAlignment = HorizontalAlignment.Stretch;
            m_tbTitle.VerticalAlignment = VerticalAlignment.Stretch;
            AddChildToGrid(m_topGrid, 0, 1, m_tbTitle);

            m_tbXlabel = GetFormattedTextBlock();
            AddChildToGrid(m_topGrid, 2, 1, m_tbXlabel);

            m_tbYlabel = GetFormattedTextBlock();
            m_tbYlabel.LayoutTransform = new RotateTransform { Angle = -90 };
            AddChildToGrid(m_topGrid, 1, 0, m_tbYlabel);

            m_textCanvas = new Canvas();
            m_textCanvas.ClipToBounds = true;
            m_textCanvas.HorizontalAlignment = HorizontalAlignment.Stretch;
            m_textCanvas.VerticalAlignment = VerticalAlignment.Stretch;
            AddChildToGrid(m_topGrid, 1, 1, m_textCanvas);

            m_chartCanvas = new Canvas();
            m_chartCanvas.ClipToBounds = true;
            m_chartCanvas.HorizontalAlignment = HorizontalAlignment.Stretch;
            m_chartCanvas.VerticalAlignment = VerticalAlignment.Stretch;
            m_textCanvas.Children.Add(m_chartCanvas);

            m_chartGrid = new Grid();
            m_chartGrid.Margin = new Thickness(0);
            m_chartGrid.ClipToBounds = true;
            m_chartGrid.Background = new SolidColorBrush(Colors.Transparent);
            m_chartGrid.SizeChanged += this.SizeChanged;
            AddChildToGrid(m_topGrid, 1, 1, m_chartGrid);
        }

        public void RenderChart()
        {
            CS = new ChartStyleGridlines();
            CS.ChartCanvas = m_chartCanvas;
            DC = new DataCollection();
            CS.Xmin = m_data.Select(l => l.Item1).Min();
            CS.Xmax = m_data.Select(l => l.Item1).Max();
            CS.Ymin = YMin.Ticks;
            CS.Ymax = YMax.Ticks;
            CS.Title = string.Empty;
            CS.YLabel = YLabel;
            CS.XLabel = string.Empty;

            CS.TextCanvas = m_textCanvas;
            CS.YTick = (new TimeSpan(1, 0, 0)).Ticks;
            if (IsTimeDuration)
            {
                CS.YTickFormatter = TicksToTimeDurationFormatter;
            }
            else
            {
                CS.YTickFormatter = TicksToTimeOfDayFormatter;
            }
            CS.GridlinePattern = ChartStyleGridlines.GridlinePatternEnum.Dot;
            CS.GridlineColor = Brushes.Black;
            DateTime t = new DateTime();
            var ts = t.AddDays(1) - t;
            CS.XTick = ts.Ticks;
            CS.XTickFormatter = TicksToDateFormatter;
            CS.AddChartStyle(m_tbTitle, m_tbXlabel, m_tbYlabel);

            DS = new DataSeries();
            DS.LineColor = Brushes.Blue;
            DS.LineThickness = 2;
            DS.Symbols.SymbolType = Symbols.SymbolTypeEnum.Circle;
            m_data.ForEach(l => DS.LineSeries.Points.Add(new Point(l.Item1, l.Item2)));
            DC.DataList.Add(DS);
            DC.AddLines(CS);
            var str = System.Windows.Markup.XamlWriter.Save(m_topGrid);
            Console.WriteLine(str);
        }

        private void SizeChanged(object sender, SizeChangedEventArgs e)
        {
            m_textCanvas.Width = m_chartGrid.ActualWidth;
            m_textCanvas.Height = m_chartGrid.ActualHeight;
            m_chartCanvas.Children.Clear();
            m_textCanvas.Children.RemoveRange(1, m_textCanvas.Children.Count - 1);
            RenderChart();
        }

        private TextBlock GetFormattedTextBlock()
        {
            var tb = new TextBlock();
            tb.Margin = new Thickness(2);
            tb.RenderTransformOrigin = new Point(0.5, 0.5);
            tb.TextAlignment = TextAlignment.Center;
            return tb;
        }

        private void AddChildToGrid(Grid grid, int row, int column, UIElement child)
        {
            Grid.SetRow(child, row);
            Grid.SetColumn(child, column);
            grid.Children.Add(child);
        }

        public static string TicksToDateFormatter(double ticks)
        {
            var dt = new DateTime((long)ticks);
            if (dt.DayOfWeek == DayOfWeek.Monday)
            {
                //return "Mon(" + dt.ToString("MM/dd") + ")";
                return "Mo(" + dt.ToString("MM/dd") + ")";
            }
            else
            {
                return dt.ToString("ddd").Substring(0, 2);
            }
        }
        public static string TicksToTimeDurationFormatter(double ticks)
        {
            var ts = new TimeSpan((long)ticks);
            return ts.ToString("hh") + " hr";
            //return ts.ToString("hh\\:mm");
        }

        public static string TicksToTimeOfDayFormatter(double ticks)
        {
            //var ts = new TimeSpan((long)ticks);
            //return ts.ToString("hh\\:mm tt");
            var ts = new TimeSpan((long)ticks);
            DateTime time = DateTime.Today.Add(ts);
            return time.ToString("h tt");

        }

        public static string MinutesToTimeFormatter(double minutes)
        {
            var ts = new TimeSpan(0, (int)minutes, 0);
            return ts.ToString("hh\\:mm"); //ToShortDateString();
        }
    }
}
