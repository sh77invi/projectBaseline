using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace DataCollectionUtility
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateWorkLog();
            CreateSleepLog();            
        }

        private static void CreateWorkLog()
        {
            var settings = new DataFileSettings();
            settings.DataDelimiter = ',';
            settings.DataEscapeChar = '"';
            settings.DataFilePath = @"C:\Users\Shahab\Documents\Cockpit\Baseline\code\web\data\input\chronus\place_events.csv";
            settings.DataHasHeader = true;
            settings.DataSchema = "Start:DateTime,End:DateTime,Offset,Place,Latitude:double,Longitude:double,Category,Distance:double,h,m";
            var chronus = new DataFileReader(settings).GetTypedDate<InputTypes.ChronusData>();

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

            var workLog = work.Select(w => string.Join("\t", new string[] {
                w.Date.ToShortDateString(), 
                w.Start.ToString(), 
                w.End.ToString(), 
                //((int)w.Duration.TotalMinutes).ToString(), 
                w.Duration.ToString(),
            })).ToList();
            workLog.Insert(0, "Date\tStart\tEnd\tDur\tDay\tStartVal\tEndVal\tDurVal");
            File.WriteAllLines(@"C:\Users\Shahab\Documents\Cockpit\Baseline\code\web\data\output\workLog.txt", workLog);
        }

        private static void CreateSleepLog()
        {
            var settings = new DataFileSettings();
            settings.DataDelimiter = ',';
            settings.DataEscapeChar = '"';
            settings.DataFilePath = @"C:\Users\Shahab\Documents\Cockpit\Baseline\code\web\data\input\toggl\toggl.csv";
            settings.DataHasHeader = true;
            settings.DataSchema = "User,Email,Client,Project,Task,Description,Billable,StartDate:DateTime,StartTime:DateTime,EndDate:DateTime,EndTime:DateTime,Duration:DateTime,Tags,Amount";
            var toggl = new DataFileReader(settings).GetTypedDate<InputTypes.Toggl>();

            var work = (from activity in toggl
                        where activity.Description == "sleep" && activity.Duration.TimeOfDay.TotalMinutes > 30
                        orderby activity.EndDate
                        select new
                        {
                            //use the EndDate, b/c to prevent problems with going to bed before/after midnight
                            Date = activity.EndDate,
                            Start = activity.StartTime.TimeOfDay,
                            End = activity.EndTime.TimeOfDay,
                            Duration = activity.Duration.TimeOfDay
                        }).ToList();

            var workLog = work.Select(w => string.Join("\t", new string[] {
                w.Date.ToShortDateString(), 
                w.Start.ToString(), 
                w.End.ToString(), 
                w.Duration.ToString(), 
            })).ToList();
            workLog.Insert(0, "Date\tStart\tEnd\tDur\tDay\tStartVal\tEndVal\tDurVal");
            File.WriteAllLines(@"C:\Users\Shahab\Documents\Cockpit\Baseline\code\web\data\output\sleepLog.txt", workLog);
        }
    }
}
