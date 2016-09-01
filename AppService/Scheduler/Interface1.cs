using System;

namespace PIReplay.Service.Scheduler
{
    public interface ICronTask
    {
        string TaskName { get; set; }
        string Frequency { get; set; }
        void ExecuteTask(object arg1, DateTime arg2, DateTime arg3);
    }
}
