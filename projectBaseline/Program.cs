using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace projectBaseline
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new DataFileSettings();
            settings.DataDelimiter = ',';
            settings.DataEscapeChar = '"';
            settings.DataFilePath = @"C:\Users\Shahab\Documents\Cockpit\Baseline\data\input\chronus\place_events.csv";
            settings.DataHasHeader = true;
            settings.DataSchema = "Start:DateTime,End:DateTime,Offset,Place,Latitude:double,Longitude:double,Category,Distance:double,h,m";
            var chronus = new DataFileReader(settings).GetTypedDate<ChronusData>();

            var work = (from activity in chronus
                        where activity.Category.ToLowerInvariant() == "work"
                        orderby activity.Start
                        group activity by activity.Start.Date into dayActivity
                        select new
                        {
                            Date = dayActivity.First().Start.Date,
                            Start = dayActivity.First().Start.TimeOfDay,
                            End = dayActivity.Last().End.TimeOfDay,
                            Duration = dayActivity.Last().End.TimeOfDay - dayActivity.First().Start.TimeOfDay
                        }).ToList();

            var workLog = work.Select(w => string.Join("\t", new string[] { w.Date.ToShortDateString(), w.Start.ToString("hh\\:mm"), w.End.ToString("hh\\:mm"), w.Duration.ToString("hh\\:mm") }));
            File.WriteAllLines(@"C:\Users\Shahab\Documents\Cockpit\Baseline\data\output\workLog.txt", workLog);

            //var work = (from row in chronus
            //            where row.Category.ToLowerInvariant() == "work"
            //            //group ((DateTime)row["Start"]).Date
            //            orderby row.Start
            //            select new
            //            {
            //                Date = row.Start.Date.ToShortDateString(),
            //                Start = row.Start.TimeOfDay,
            //                End = row.End.TimeOfDay,
            //                Duration = row.End.TimeOfDay - row.Start.TimeOfDay
            //            }).ToList();

            var s = work.ToString();
        }
    }
}
