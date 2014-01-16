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
        private ChartStyle cs;
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


            cs = new ChartStyle();
            cs.ChartCanvas = chartCanvas;
            dc = new DataCollection();
            cs.Xmin = workLog.Select(l => l.Date.Ticks).Min();
            cs.Xmax = workLog.Select(l => l.Date.Ticks).Max();
            cs.Ymin = 9 * 60;
            cs.Ymax = 14 * 60;

            // Draw Sine curve:
            ds = new DataSeries();
            ds.LineColor = Brushes.Blue;
            ds.LineThickness = 2;
            workLog.ForEach(l => ds.LineSeries.Points.Add(new Point(l.Date.Ticks, l.Start.Hour * 60 + l.Start.Minute)));

            dc.DataList.Add(ds);
            dc.AddLines(cs);
        }

        private void chartGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }
    }
}
