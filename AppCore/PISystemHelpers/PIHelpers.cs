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

namespace PIReplay.Core.PISystemHelpers
{
    public static class PIHelpers
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PIHelpers));

        public static void DeleteValues(PIPoint piPoint, AFTime st, AFTime et)
        {
            var timerange = new AFTimeRange(st, et);

            if (piPoint.Server.Supports(PIServerFeature.DeleteRange))
            {
                var errors = piPoint.ReplaceValues(timerange, new List<AFValue>());

                if (errors != null && errors.HasErrors)
                {
                    _logger.Error(errors.Errors);
                    _logger.Error(errors.PIServerErrors);
                }
            }

            else
            {   // fallback on the old fashion way of doing the delete
                // if code goes here this may be much slower to delete the values.
                _logger.Warn("The ReplaceValues method is not implemented on the target PI DataArchive, falling back to the previous longer method ( read and remove)");
                var dataToDelete = piPoint.RecordedValues(timerange, AFBoundaryType.Inside, string.Empty, false);
                piPoint.UpdateValues(dataToDelete, AFUpdateOption.Remove);

            }



        }

        public static void RecordedBulkParallelByChunks(    
            AFTimeRange timeRange,
            int maxDegOfParallel,
            int bulkParallelChunkSize,
            int bulkPageSize,
            bool insertNoData,
            Action<List<AFValue>> insertData,
            PIPointsProvider provider,
            int sleep,
            CancellationToken cancelToken
            )
        {
            var conf = Settings.PISystemHelpers.Default;
            int i = 0;

            foreach (var pointsChunk in provider.GetPointsByChunks(conf.RecordedBulkParallelTagsPerCall))
            {
                i++;
                _logger.InfoFormat("Processing tag chunk {0} - (tag chunk size: {1})", i, conf.RecordedBulkParallelTagsPerCall);
                var pointsList = new PIPointList(pointsChunk);

                RecordedBulkParallel(pointsList, timeRange, 4, conf.RecordedBulkParallelTagsPerCall, conf.BulkPageSize, insertNoData, insertData, new CancellationToken());

                if (sleep > 0)
                    Thread.Sleep(sleep);

            }
        }


        /// <summary>
        /// This method splits a point list into severall smaller lists and perform bulk calls on each list
        /// In parallel.  
        /// </summary>
        private static void RecordedBulkParallel(
            IEnumerable<PIPoint> PiPoints,
            AFTimeRange timeRange,
            int maxDegOfParallel,
            int bulkParallelChunkSize,
            int bulkPageSize,
            bool insertNoData,
            Action<List<AFValue>> insertData,
            CancellationToken cancelToken)
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

                        List<AFValue> singleListData = new List<AFValue>();


                        // inserting no data when backfilling if no data is found in the period
                        if (insertNoData)
                        {
                            foreach (var valuesInPoint in data)
                            {
                                if (valuesInPoint.Count == 0)
                                {

                                    var value = new AFValue();
                                    value.PIPoint = valuesInPoint.PIPoint;
                                    value.Value = "No Data";
                                    value.Timestamp = timeRange.EndTime;
                                    singleListData.Add(value);
                                }

                            }

                        }


                        singleListData.AddRange(data.SelectMany(x => x).ToList());

                        if (singleListData.Count == 0)
                        {
                            return;
                        }

                        insertData(singleListData);

                        _logger.DebugFormat("returned {0} values to be written", singleListData.Count);


                    }
                    catch (OperationCanceledException ex)
                    {
                        _logger.Error(ex);
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
