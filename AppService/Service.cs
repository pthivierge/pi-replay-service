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
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using log4net;
using PIReplay.Core;
using PIReplay.Service.Scheduler;
using PIReplay.Settings;

namespace PIReplay.Service
{
    public partial class Service : ServiceBase
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (Service));
        Replayer _replayer=null;

        /// Scheduler that handles exact time scheduling
        private readonly CronScheduler _scheduler = new CronScheduler();

        #region Constructors

        /// <summary>
        ///     constructeur du service
        ///     Fist byte of code called when service is started
        /// </summary>
        public Service()
        {
            try
            {
                _logger.Info("Service is Initializing...");
                InitializeComponent();

                // do not put your code here... put it in on start, otherwise if the service crashes you wont see anything
            }


            catch (Exception e)
            {
                _logger.Fatal(e);
                throw;
            }
        }

        #endregion

        #region Readonly & Static Fields

        #endregion

        #region Instance Methods

        protected override void OnShutdown()
        {
            // your code here

            base.OnShutdown();
            _logger.Info("Service is shutting down.");
        }

        protected override void OnStart(string[] args)
        {
            
            try
            {
                if (Advanced.Default.StartDebuggerOnStart)
                {
                    Debugger.Launch();

                    while (!Debugger.IsAttached)
                    {
                        // Waiting until debugger is attached
                        RequestAdditionalTime(1000); // Prevents the service from timeout
                        Thread.Sleep(1000); // Gives you time to attach the debugger   
                    }
                    RequestAdditionalTime(20000); // for Debugging the OnStart method,
                    // increase as needed to prevent timeouts
                }

                base.OnStart(args);

                // put your startup service code here:

                _logger.Info("Service is Started.");

                ValidateSettings();
                //  this scheduler was part of the app template.
                // not used for now, but could be a nice and easy way to schedule other tasks in the service
                //  initScheduler();
                _replayer =new Replayer();
                

                _replayer.Run(General.Default.ServerName, General.Default.TagQueryString);

            }
            catch (Exception exception)
            {
                _logger.Fatal(exception);
            }
        }

        private void ValidateSettings()
        {
            if(string.IsNullOrEmpty(General.Default.TagQueryString))
                throw new Exception("The parameter TagQueryString cannot be empty.");

        }

        protected override void OnStop()
        {
            // your code here
            _replayer.Stop();

            base.OnStop();
            _logger.Info("Service Stopped.");
        }


        private void initScheduler()
        {
            //string dailyReportCronExp = Settings.General.Default.BasicTasksSchedule;
            //_scheduler.AddTask("BaseTask", dailyReportCronExp, BasicTask);

            //_scheduler.Start();
        }

        private void BasicTask()
        {
            _logger.Info("The task is doing a lot of work !");
        }

        #endregion
    }
}
