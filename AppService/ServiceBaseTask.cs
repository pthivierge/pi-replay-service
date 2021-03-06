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
using System.Threading;
using System.Timers;
using log4net;
using Timer = System.Timers.Timer;

namespace PIReplay.Service
{
    /// <summary>
    ///     This class allows to configure a service tasks that runs at regular intervals
    /// </summary>
    public class ServiceBaseTask
    {
        #region Constructors

        public ServiceBaseTask(Action task, int pollIntervalms = 10000, string taskName = "")
        {
            _taskName = taskName;
            Logger.Debug("Creating task " + TaskName + " - Interval ms= " + pollIntervalms);
            PollingInteval = pollIntervalms;
            _task = task;
        }

        public ServiceBaseTask(Action task, TimeSpan time, string taskName = "")
        {
            _taskName = taskName;
            PollingInteval = time.TotalMilliseconds;
            _task = task;
        }

        #endregion

        #region Readonly & Static Fields

        private static readonly ILog Logger = LogManager.GetLogger(typeof (ServiceBaseTask));

        private readonly Action _task;
        private readonly string _taskName;

        #endregion

        #region Fields

        private double _pollingInteval;
        private Timer _timer = new Timer();

        #endregion

        #region Instance Properties

        /// <summary>
        ///     polling period in ms
        /// </summary>
        public double PollingInteval
        {
            get { return _pollingInteval; }
            set
            {
                _pollingInteval = value;
                _timer.Interval = _pollingInteval;
            }
        }

        public string TaskName
        {
            get { return _taskName; }
        }

        #endregion

        #region Instance Methods

        public bool IsStarted()
        {
            return (_timer != null);
        }


        public void Start()
        {
            Logger.Debug("Starting task: " + TaskName);
            _timer.AutoReset = true;
            _timer.Interval = _pollingInteval;
            _timer.Elapsed += OnTimerProc;

            _timer.Start();

            // selon la tache il faudra peut �tre ajuster ou parametrer cette intervalle
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            _task();
        }

        public void Stop()
        {
            Logger.Debug("Stopping task: " + TaskName);
            _timer.Dispose();
            _timer = null;
        }

        #endregion

        #region Event Handling

        private void OnTimerProc(object source, ElapsedEventArgs e)
        {
            // executes the task 
            Logger.Debug("Executing task: " + TaskName);
            _task();
        }

        #endregion

        // _logger pour �crire les logs
    }
}
