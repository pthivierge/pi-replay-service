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
using PIReplay.Core.Helpers;
using Timer = System.Timers.Timer;


namespace PIReplay.Core
{
    public class DataReader
    {
        private DateTime _nextStartTime;
        private readonly ILog _logger = LogManager.GetLogger(typeof(DataReader));
        readonly PIPointsProvider _pointsProvider = null;
        private readonly BlockingCollection<List<AFValue>> _writeQueue = null;
        private Timer _timer = new Timer();
        private object _timerLock = new object();
        private TimeSpan _timeOffset;

        private bool _backfill = false;

        public DataReader(PIPointsProvider pointsProvider, BlockingCollection<List<AFValue>> writeQueue)
        {
            _pointsProvider = pointsProvider;
            _writeQueue = writeQueue;
            _timeOffset= TimeSpan.FromDays(Settings.General.Default.ReplayTimeOffsetDays);
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
            var conf = Settings.General.Default;

            try
            {
                
                
                

                // if we are not backfilling, we are using the _nextStartTime, if we backfill, we make a data call to get it from the data.
                DateTime startTime;
                DateTime endTime;
                var timeRanges = DefineTimes(_pointsProvider, out startTime, out endTime);
                
                // inserts the data, sorted by time
                foreach (var timeRange in timeRanges)
                {
                    var i = 0;
                    _logger.InfoFormat("Reading the data for time range: {0} to {1}", timeRange.StartTime,timeRange.EndTime);

                    foreach (var pointsChunk in _pointsProvider.GetPointsByChunks(conf.TagsChunkSize))
                    {
                        i++;
                        _logger.InfoFormat("Processing tag chunk {0} - (tag chunk size: {1})",i, conf.TagsChunkSize);
                        var pointsList = new PIPointList(pointsChunk);

                        GetRecordedValuesBulkParrallel(pointsList,timeRange, conf.BulkPageSize,4,conf.BulkParallelChunkSize, new CancellationToken());

                    }
                    
                }


            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }



        private List<AFTimeRange> DefineTimes(PIPointsProvider provider, out DateTime startTime, out DateTime endTime)
        {
            startTime=DateTime.MaxValue;
            

            // when backfilling, we will get the oldest timestamp in the snapshot
            if (_backfill)
            {
                int i = 0;
                foreach (var tagChunk in provider.GetPointsByChunks(Settings.General.Default.TagsChunkSize))
                {
                    i++;
                    _logger.InfoFormat("Looking for minimum snapshot time in the data for tag chunk: {0} - chunk size: {1}", i, Settings.General.Default.TagsChunkSize);
                    var pointsList=new PIPointList(tagChunk);
                    var time = pointsList.CurrentValue().Min(v => v.Timestamp.LocalTime) - _timeOffset;
                    startTime = time < startTime ? time : startTime;
                }

                _logger.InfoFormat("Backfill start time found {0:G}",startTime.ToLocalTime());

                
            }
            else
            {
                startTime = _nextStartTime;
            }

            endTime = DateTime.Now - _timeOffset;
            _nextStartTime = endTime.AddSeconds(1);
            var timeRanges = GetTimePeriods(startTime, endTime);
            return timeRanges;
        }

        private List<AFTimeRange> GetTimePeriods(DateTime start, DateTime end)
        {
            var datesIntervals = TimeStampsGenerator.GetAfTimeRanges(TimeSpan.FromHours(Settings.General.Default.BackFillHoursPerDataChunk), start, end);
            return datesIntervals;
        }

        /// <summary>
        /// This method splits a point list into severall smaller lists and perform bulk calls on each list
        /// In parallel.  
        /// </summary>
        private void GetRecordedValuesBulkParrallel(IEnumerable<PIPoint> PiPoints, AFTimeRange timeRange, int bulkPageSize, int maxDegOfParallel, int bulkParallelChunkSize, CancellationToken cancelToken)
        {

            _logger.WarnFormat("QUERY (BULK-P) # - PERIOD: {3} to {4} - MAX DEG. PAR. {0}, TAG_CHUNK_SIZE {1}, TAG_PAGE_SIZE {2},", maxDegOfParallel, bulkParallelChunkSize, bulkPageSize, timeRange.StartTime, timeRange.EndTime);

            // PARALLEL bulk 
            var pointListList = PiPoints.ToList().ChunkBy(bulkParallelChunkSize);
            Parallel.ForEach(pointListList, new ParallelOptions { MaxDegreeOfParallelism = maxDegOfParallel, CancellationToken = cancelToken },
                (pts, state, index) =>
                {

                    PIPagingConfiguration pagingConfiguration = new PIPagingConfiguration(PIPageType.TagCount, bulkPageSize);
                    PIPointList pointList = new PIPointList(pts);

                    try
                    {
                        // _logger.InfoFormat("Bulk query");
                        IEnumerable<AFValues> data = pointList.RecordedValues(timeRange,
                            AFBoundaryType.Inside, String.Empty, false, pagingConfiguration).ToList();

                        
                        _logger.InfoFormat("READ Recorded values between {0:G} and {1:G}. {2} values found", timeRange.StartTime.LocalTime, timeRange.EndTime.LocalTime, data.Sum(x => x.Count));


                        var singleListData = data.SelectMany(x => x).ToList();

                        if (singleListData.Count == 0)
                            return;

                        // re-adjusting the timestamp before writing it to the PI Data Archive
                        singleListData.ForEach(v =>
                        {
                            v.Timestamp = v.Timestamp + _timeOffset;
                        });


                        _writeQueue.Add(singleListData);
                        _logger.DebugFormat("QUEUED {0} values to be written", singleListData.Count);


                    }
                    catch (OperationCanceledException ex)
                    {
                        _logger.Error(pagingConfiguration.Error);
                    }
                    catch (Exception ex)
                    {

                        _logger.Error(ex);

                    }



                });


        }


    }
}
