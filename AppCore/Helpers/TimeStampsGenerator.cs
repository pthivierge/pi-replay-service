using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using OSIsoft.AF.Time;

namespace PIReplay.Core
{


    public static class TimeStampsGenerator
    {

        private static ILog _logger = LogManager.GetLogger(typeof(TimeStampsGenerator));

        public static List<DateTime> GetDateTimes(TimeSpan interval, DateTime startTime, DateTime endTime)
        {

            var dates = new List<DateTime>();

            var currentTime = startTime;
            while (currentTime < endTime)
            {
                dates.Add(currentTime);
                currentTime = currentTime.AddSeconds(interval.TotalSeconds);
            }

            dates.Add(endTime);

            return dates;
        }

        public static List<AFTimeRange> GetAfTimeRanges(TimeSpan interval, DateTime startTime, DateTime endTime)
        {

            var dates = new List<AFTimeRange>();
            var dateTimes = new List<DateTime>();
            var currentTime = startTime;
            while (currentTime < endTime)
            {
                dateTimes.Add(currentTime);
                currentTime = currentTime.AddSeconds(interval.TotalSeconds);
            }

            dateTimes.Add(endTime);

            for (var i = 0; i < dateTimes.Count - 1; i++)
            {
                var st = dateTimes[i];
                var et = dateTimes[i + 1];

                // removes 1 second if not the last end time, to avoid overlapping and query same recorded event twice.
                if (i+1 != dateTimes.Count - 1)
                    et = et.AddSeconds(-1);

                dates.Add(new AFTimeRange(st,et));

                _logger.DebugFormat("Timerange between {0} and {1}",st,et);
            }

            return dates;

        }

    }

}
