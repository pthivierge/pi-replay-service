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
using log4net;
using Quartz;

namespace PIReplay.Service.Scheduler
{
    public sealed class CronTask : IJob
    {
        private readonly string _cronConfig;
        private readonly ILog _logger = LogManager.GetLogger(typeof (CronTask));
        private readonly Action _task;
        private readonly string _taskName;

        public CronTask()
        {
        }

        public CronTask(string taskName, string cronConfig, Action action)
        {
            _taskName = taskName;
            _task = action;
            _cronConfig = cronConfig;
        }

        public string TaskName
        {
            get { return _taskName; }
        }

        public string CronConfig
        {
            get { return _cronConfig; }
        }

        public void Execute(IJobExecutionContext context)
        {
            var task = (CronTask) context.MergedJobDataMap.Get("task");

            // executes the task 
            _logger.Info("Executing task : " + task.TaskName);
            task._task();
        }
    }
}
