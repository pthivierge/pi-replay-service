using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;
using PIReplay.Settings;
using Timer = System.Timers.Timer;


namespace PIReplay.Core
{
    public class DataReader
    {
        private DateTime _nextStartTime;
        private readonly ILog _logger = LogManager.GetLogger(typeof(DataReader));
        readonly PIPointsProvider _pointsProvider = null;
        private readonly BlockingCollection<List<AFValue>> _writeQueue = null;
        Timer _timer = new System.Timers.Timer();
        private object _timerLock = new object();

        private bool _backfill = false;

        public DataReader(PIPointsProvider pointsProvider, BlockingCollection<List<AFValue>> writeQueue)
        {
            _pointsProvider = pointsProvider;
            _writeQueue = writeQueue;
        }

        public void RunBackfill()
        {
            _logger.Info("Running backfill");
            _backfill = true;
            ReadData();
            _backfill = false;
            _logger.Info("Backfill Completed");
        }

        public void Stop()
        {
            Monitor.Enter(_timerLock); // wait for the current task to complete
            _timer.Stop();
        }


        /// <summary>
        /// Starts the data collection mecanism
        /// </summary>
        /// <param name="frequency">The time interval in seconds at which the Data will be read</param>
        public void Run(int frequency)
        {
            _logger.Info("Starting the data collection process");

            _timer.Elapsed += (source, e) =>
            {
                // here is a little logic to avoid the timer starting reading again if previous run was no completed.
                try
                {

                    if (!Monitor.TryEnter(_timerLock))
                    {
                        // something has the lock. Probably shutting down.
                        return;
                    }

                    _timer.Stop();
                    ReadData();

                }
                finally
                {
                    Monitor.Exit(_timerLock);
                    _timer.Start();
                }

            };

            _timer.Interval = frequency * 1000;
            _timer.Start();

        }



        private void ReadData()
        {

            try
            {

                // find the snapshot at which to start the backfill
                int i = 0;
                foreach (var pointsChunk in _pointsProvider.GetPointsByChunks(General.Default.TagsChunkSize))
                {
                    i++;
                    _logger.InfoFormat("Reading the data for tag chunk: {0} - chunk size: {1}", i, General.Default.TagsChunkSize);

                    var pointsList = new PIPointList(pointsChunk);
                    var timeOffset = TimeSpan.FromDays(General.Default.ReplayTimeOffsetDays);

                    // if we are not backfilling, we are using the _nextStartTime, if we backfill, we make a data call to get it from the data.
                    var startTime = _backfill ? pointsList.CurrentValue().Min(v => v.Timestamp.LocalTime) - timeOffset : _nextStartTime;
                    var endTime = DateTime.Now - timeOffset;
                    _nextStartTime = endTime.AddSeconds(1);

                    var timeRanges = GetTimePeriods(startTime, endTime);

                    // inserts the data, sorted by time
                    Parallel.ForEach(timeRanges, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, (tr) =>
                       {

                           // get the data for each time period
                           var data = pointsList.RecordedValues(tr, AFBoundaryType.Inside, null, false,
                                      new PIPagingConfiguration(PIPageType.TagCount, 1000)).ToList();

                           _logger.InfoFormat("READ Recorded values between {0:G} and {1:G}. {2} values found", tr.StartTime.LocalTime, tr.EndTime.LocalTime, data.Sum(x => x.Count));


                           var singleListData = data.SelectMany(x => x).ToList();

                           if(singleListData.Count==0)
                               return;

                           // re-adjusting the timestamp before writing it to the PI Data Archive
                           singleListData.ForEach(v =>
                           {
                               v.Timestamp = v.Timestamp + timeOffset;
                           });


                           _writeQueue.Add(singleListData);
                           _logger.DebugFormat("QUEUED {0} values to be written",singleListData.Count);

                       });
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private List<AFTimeRange> GetTimePeriods(DateTime start, DateTime end)
        {
            var datesIntervals = TimeStampsGenerator.GetAfTimeRanges(TimeSpan.FromHours(General.Default.BackFillHoursPerDataChunk), start, end);
            return datesIntervals;
        }


    }
}
