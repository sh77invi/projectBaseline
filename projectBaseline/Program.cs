using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
//using System.Data.DataSetExtensions;

namespace projectBaseline
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new DataFileSettings();
            settings.DataDelimiter = ',';
            settings.DataEscapeChar = '"';
            settings.DataFilePath = @"C:\Users\Shahab\Documents\Cockpit\Baseline\data\chronus\place_events.csv";
            settings.DataHasHeader = true;
            settings.DataSchema = "Start:DateTime,End:DateTime,Offset,Place,Latitude:double,Longitude:double,Category,Distance:double,h,m";
            var chronus = new DataFileReader(settings);


            var work = (from DataRow row in chronus.Data.Rows
                        where ((string)row["Category"]).ToLowerInvariant() == "work"
                        //group ((DateTime)row["Start"]).Date
                        orderby ((DateTime)row["Start"])
                        select new
                        {
                            Date = ((DateTime)row["Start"]).Date,
                            Start = row.Field<DateTime>("Start").TimeOfDay,
                            End = row.Field<DateTime>("End").TimeOfDay,
                            Duration = row.Field<DateTime>("End").TimeOfDay - row.Field<DateTime>("Start").TimeOfDay
                        }).ToList();


            var c = work;
        }
    }
}
