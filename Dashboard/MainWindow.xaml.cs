using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChartingUtility.LineCharts;
using DataCollectionUtility;

namespace Dashboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChartStyleGridlines cs;
        //private ChartStyle cs;
        private DataCollection dc;
        private DataSeries ds;

        public MainWindow()
        {
            InitializeComponent();

            var settings = new DataFileSettings();
            settings.DataDelimiter = '\t';
            settings.DataEscapeChar = '$'; //fix this
            settings.DataFilePath = @"C:\Users\Shahab\Documents\Cockpit\Baseline\code\web\data\output\workLog.txt";
            settings.DataHasHeader = true;
            settings.DataSchema = "Date:DateTime,Start:DateTime,End:DateTime,Duration:int";
            var workLog = new DataFileReader(settings).GetTypedDate<DataCollectionUtility.OutputTypes.TimeLog>();

            LineChart chart;

            chart = new LineChart(workStartGrid, workLog.Select(l => new Tuple<DateTime, TimeSpan>(l.Date, l.Start.TimeOfDay)));
            chart.YMin = TimeSpan.FromHours(9);
            chart.YMax = TimeSpan.FromHours(14);
            chart.YLabel = "Work Start";

            chart = new LineChart(workEndGrid, workLog.Select(l => new Tuple<DateTime, TimeSpan>(l.Date, l.End.TimeOfDay)));
            chart.YMin = TimeSpan.FromHours(18);
            chart.YMax = TimeSpan.FromHours(22);
            chart.YLabel = "Work End";

            chart = new LineChart(workDurGrid, workLog.Select(l => new Tuple<DateTime, TimeSpan>(l.Date, new TimeSpan(0, l.Duration, 0))));
            chart.YMin = TimeSpan.FromHours(6);
            chart.YMax = TimeSpan.FromHours(11);
            chart.YLabel = "Hours Worked";
            chart.IsTimeDuration = true;
        }
    }
}
