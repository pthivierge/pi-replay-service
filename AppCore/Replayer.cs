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
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using PIReplay.Core.Data;
using PIReplay.Settings;

namespace PIReplay.Core
{
    public class Replayer
    {

        private static readonly ILog _logger = LogManager.GetLogger(typeof(Replayer));
        private DataReader _dataReader = null;
        private DataWriter _dataWriter = null;

        private BlockingCollection<DataPacket> _queue=new BlockingCollection<DataPacket>();


        public Replayer()
        {
            
        }

        public void RunFromCommandLine(string server, string pointsQuery)
        {
            this.Run(server, pointsQuery);
           _logger.Info("Press a key to stop the application.");
           Console.ReadKey();
            Stop();

        }

        public void Run(string server, string pointsQuery)
        {
            var connection=new PIConnection(server);
            var pointsProvier=new PIPointsProvider(pointsQuery,connection.GetPiServer());

            _dataReader = new DataReader(pointsProvier, _queue);
            _dataWriter = new DataWriter(_queue, connection.GetPiServer());
            _dataWriter.Run();
            
            _dataReader.RunBackfill();

            _logger.Info("Starting the normal operations process");
            _dataReader.Run(General.Default.DataCollectionFrequencySeconds);

        }

        public void Stop()
        {
            _dataReader.Stop();
            _dataWriter.Stop();
            _logger.Info("Application Stopped");
        }


    }
}
