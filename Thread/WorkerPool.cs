using System;
using System.Diagnostics;
using TsGui.Collections;

namespace TsGui.Thread
{
    public enum SelectQueueMode
    {
        Sequential,
        JobCount,
    }

    public class WorkerPool : IDisposable
    {
        private readonly IWorkerQueue[] _workerQueue;

        public SelectQueueMode SelectQueueMode { get; set; }

        public WorkerPool(int workerCount ,Action killed = null , Func<IWorkerQueue> workerGenerator = null)
        {
            Debug.Assert(workerCount > 0);
            _workerQueue = new IWorkerQueue[workerCount];
            var generator = workerGenerator ?? (() => new WorkerThread());

            for (var i = 0; i < workerCount; ++i)
            {
                _workerQueue[i] = generator();
                if(killed != null)
                    _workerQueue[i].Killed += killed;
            }
        }

        public void AddJob(Action job , JobPriority priority = JobPriority.Default)
        {
            if(SelectQueueMode == SelectQueueMode.Sequential)
                GetNextWorkerQueue().AddJob(job, priority);
            else if(SelectQueueMode == SelectQueueMode.JobCount)
                GetLessWorkerQueue().AddJob(job, priority);
        }

        private InfiniteList<IWorkerQueue> _infiniteList;
        private IWorkerQueue GetNextWorkerQueue()
        {
            if(_infiniteList is null)
                _infiniteList = _workerQueue.ToInfiniteList();

            return _infiniteList.Yield();
        }

        private IWorkerQueue GetLessWorkerQueue()
        {
            return _workerQueue.FindMin(x => x.JobsCount);
        }

        private bool _disposed;
        public void Dispose()
        {
            if(_disposed)
                return;
            _disposed = true;
            foreach (var queue in _workerQueue)
                queue.Kill();
        }
    }
}
