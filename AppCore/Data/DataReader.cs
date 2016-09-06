#region Copyright
//  Copyright 2016 Patrice Thivierge F.
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion
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
using PIReplay.Core.Data;
using PIReplay.Settings;
using PIReplay.Core.PISystemHelpers;
using Timer = System.Timers.Timer;


namespace PIReplay.Core
{
    public class DataReader
    {
        private DateTime _nextStartTime;
        private readonly ILog _logger = LogManager.GetLogger(typeof(DataReader));
        readonly PIPointsProvider _pointsProvider = null;
        private readonly BlockingCollection<DataPacket> _writeQueue = null;
        private Timer _timer = new Timer();
        private object _timerLock = new object();
        private TimeSpan _timeOffset;
        private CancellationToken _cancellationToken;
        

        private bool _backfill = false;

        public DataReader(PIPointsProvider pointsProvider, BlockingCollection<DataPacket> writeQueue, CancellationToken token)
        {
            _pointsProvider = pointsProvider;
            _writeQueue = writeQueue;
            _timeOffset = TimeSpan.FromDays(Settings.General.Default.ReplayTimeOffsetDays);
            _cancellationToken = token;
        }

        /// <summary>
        /// This is the first method of this class that should be called.
        /// </summary>
        public void RunBackfill()
        {
            _logger.Info("Running backfill");
            _backfill = true;
            var timeRanges = GetTimeRanges(_pointsProvider);

            var conf = Settings.General.Default;

            // inserts the data, sorted by time
            foreach (var timeRange in timeRanges)
            {

                _logger.InfoFormat("Reading the data for time range: {0:G} to {1:G}", timeRange.StartTime.LocalTime, timeRange.EndTime.LocalTime);

                PIHelpers.RecordedBulkParallelByChunks(
                    timeRange,
                    conf.BackfillMaxReadThreadCountMDP,
                    conf.BackfillBulkParallelChunkSize,
                    conf.BackfillBulkPageSize,
                    true,
                    (data) =>
                    {
                        // re-adjusting the timestamp before writing it to the PI Data Archive
                        data.ForEach(v =>
                    {
                        v.Timestamp = v.Timestamp + _timeOffset;
                    });

                        _writeQueue.Add(
                            new DataPacket()
                            {
                                Data = data
                            });
                    },
                    _pointsProvider,
                    0,
                    _cancellationToken
                    );

            }


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

                    var timeRanges = GetTimeRanges(_pointsProvider);

                    // inserts the data, sorted by time
                    foreach (var timeRange in timeRanges)
                    {

                        _logger.InfoFormat("Reading the data for time range: {0:G} to {1:G}", timeRange.StartTime.LocalTime, timeRange.EndTime.LocalTime);

                        if (_cancellationToken.IsCancellationRequested)
                        {
                            _logger.InfoFormat("Cancellation requested, stopping the data collection");
                            return;
                        }
                            

                        PIHelpers.RecordedBulkByChunks(
                            timeRange,
                            Settings.General.Default.NormalOpTagsChunkSize,
                            false,
                            (data) =>
                            {
                                // re-adjusting the timestamp before writing it to the PI Data Archive
                                data.ForEach(v =>
                                        {
                                            v.Timestamp = v.Timestamp + _timeOffset;
                                        });

                                _writeQueue.Add(
                                    new DataPacket()
                                    {
                                        Data = data
                                    });
                            },
                            _pointsProvider,
                            Settings.General.Default.NormalSleepTimeBetweenChunks,
                            _cancellationToken
                            );

                    }

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




        /// <summary>
        /// todo: needs refactoring to remove the if and separate the logic between backfill and not backfill
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        private List<AFTimeRange> GetTimeRanges(PIPointsProvider provider)
        {

            DateTime startTime;
            DateTime endTime;

            // when backfilling, we will get the oldest timestamp in the snapshot
            if (_backfill)
            {
                startTime = DateTime.MaxValue;
                int i = 0;
                foreach (var tagChunk in provider.GetPointsByChunks(Settings.General.Default.BackfillTagsChunkSize))
                {
                    i++;
                    _logger.InfoFormat("Looking for minimum snapshot time in the data for tag chunk: {0} - chunk size: {1}", i, Settings.General.Default.BackfillTagsChunkSize);
                    var pointsList = new PIPointList(tagChunk);
                    var time = pointsList.CurrentValue().Min(v => v.Timestamp.LocalTime);
                    startTime = time < startTime ? time : startTime;
                }


                if (startTime < DateTime.Now - TimeSpan.FromDays(Settings.General.Default.ReplayTimeOffsetDays))
                    startTime = Settings.General.Default.BackfillDefaultStartTime;


                endTime = DateTime.Now.ToLocalTime();
                _nextStartTime = endTime.AddSeconds(1);

                _logger.InfoFormat("Backfill start time: {0:G} - end time: {1:G}", startTime.ToLocalTime(), endTime.ToLocalTime());

            }
            else
            {
                startTime = _nextStartTime;
                endTime = DateTime.Now;
                _nextStartTime = endTime.AddSeconds(1);
            }


            var timeRanges =
                TimeStampsGenerator.GetAfTimeRanges(
                    TimeSpan.FromHours(Settings.General.Default.BackFillHoursPerDataChunk)
                    , startTime - _timeOffset
                    , endTime - _timeOffset);

            return timeRanges;
        }





    }
}
