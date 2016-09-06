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

        public static void DeleteValues(PIPoint piPoint, AFTime st, AFTime et, bool forceUpdateValuesMethod=false)
        {
            var timerange = new AFTimeRange(st, et);

            if (piPoint.Server.Supports(PIServerFeature.DeleteRange) && forceUpdateValuesMethod==false)
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
                _logger.Warn("The ReplaceValues method is not implemented on the target PI DataArchive or the forceUpdateValuesMethod flag was used, falling back to the previous longer method ( read and remove)");
                
                var dataToDelete = piPoint.RecordedValues(timerange, AFBoundaryType.Inside, string.Empty, false);

                if(dataToDelete.Count>0)
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

            int i = 0;

            foreach (var pointsChunk in provider.GetPointsByChunks(bulkParallelChunkSize))
            {
                i++;
                _logger.InfoFormat("Processing tag chunk {0} - (tag chunk size: {1})", i, bulkParallelChunkSize);
                var pointsList = new PIPointList(pointsChunk);

                RecordedBulkParallel(pointsList, timeRange, 4, bulkParallelChunkSize, bulkPageSize, insertNoData, insertData, cancelToken);

                if (sleep > 0)
                    Thread.Sleep(sleep);

            }
        }

        public static void RecordedBulkByChunks(
          AFTimeRange timeRange,
          int pointsChunkSize,
          bool insertNoData,
          Action<List<AFValue>> insertData,
          PIPointsProvider provider,
          int sleep,
          CancellationToken cancellationToken
          )
        {

            int i = 0;

            foreach (var pointsChunk in provider.GetPointsByChunks(pointsChunkSize))
            {
                i++;
                _logger.InfoFormat("Processing tag chunk {0} - (tag chunk size: {1})", i, pointsChunkSize);
                var pointsList = new PIPointList(pointsChunk);

                RecordedBulk(pointsList, timeRange, pointsChunkSize, insertNoData, insertData); // todo needs a different setting here

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
                                    var value = AFValue.CreateSystemStateValue(AFSystemStateCode.NoData, timeRange.EndTime);
                                    value.PIPoint = valuesInPoint.PIPoint;
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


        private static void RecordedBulk(
    IEnumerable<PIPoint> PiPoints,
    AFTimeRange timeRange,
    int bulkPageSize,
    bool insertNoData,
    Action<List<AFValue>> insertData)
        {

            _logger.WarnFormat("QUERY (BULK) # - PERIOD: {1} to {2}, TAG_PAGE_SIZE {0},", bulkPageSize, timeRange.StartTime, timeRange.EndTime);

            
                    PIPagingConfiguration pagingConfiguration = new PIPagingConfiguration(PIPageType.TagCount, bulkPageSize);
                    PIPointList pointList = new PIPointList(PiPoints);

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
                                    var value = AFValue.CreateSystemStateValue(AFSystemStateCode.NoData, timeRange.EndTime);
                                    value.PIPoint = valuesInPoint.PIPoint;
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



        }

    }
}
