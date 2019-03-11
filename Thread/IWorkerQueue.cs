
using System;

namespace TsGui.Thread
{
    public enum JobPriority
    {
        Top,
        Default,
        Background,
    }

    public interface IWorkerQueue
    {
        event Action Killed;

        int JobsCount { get; }
        void AddJob(Action action, JobPriority priority);
        void Kill();
    }
}