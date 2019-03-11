using System;
using System.Collections.Concurrent;
using System.Linq;

namespace TsGui.Thread
{
    public class WorkerThread : IWorkerQueue
    {
        private readonly ConcurrentDictionary<JobPriority,ConcurrentStack<Action>> _jobs = new ConcurrentDictionary<JobPriority, ConcurrentStack<Action>>();
        public event Action Killed;
        public int JobsCount => _jobs.Count;

        private bool _isRunning;
        public bool IsRunning => _isRunning;

        private TimeSpan AutoRemoveTimeSpan { get; }
        private TimeSpan AutoUpdateTimeSpan { get; }

        public static TimeSpan DefaultUpdateTimeSpan { get; set; } = TimeSpan.FromMilliseconds(1);
        public static TimeSpan DefaultAutoRemoveTimeSpan { get; set; } = TimeSpan.MaxValue;

        public WorkerThread()
        {
            AutoRemoveTimeSpan = DefaultAutoRemoveTimeSpan;
            AutoUpdateTimeSpan = DefaultUpdateTimeSpan;
        }

        public WorkerThread(TimeSpan autoRemoveTimeSpan , TimeSpan autoUpdateTimeSpan)
        {
            AutoRemoveTimeSpan = autoRemoveTimeSpan;
            AutoUpdateTimeSpan = autoUpdateTimeSpan;
        }

        private void RunInternal()
        {
            if (_isRunning)
                return;
            _isRunning = true;
            var thread = new System.Threading.Thread(() =>
            {
                var updateCheck = DateTime.Now;
                while (_isRunning)
                {
                    if (DateTime.Now - updateCheck >= AutoUpdateTimeSpan)
                    {
                        foreach (var priority in Enum.GetValues(typeof(JobPriority)).OfType<JobPriority>())
                        {
                            if (_jobs.ContainsKey(priority))
                            {
                                _jobs[priority].TryPop(out var action);
                                if (action != null)
                                {
                                    updateCheck = DateTime.Now;
                                    action.Invoke();
                                    break;
                                }
                            }
                        }
                    }
                    
                    //! 仕事が無い状態で一定時間経過すればスレッドを終了する
                    if (JobsCount == 0 && DateTime.Now - updateCheck >= AutoRemoveTimeSpan)
                        break;
                }

                _isRunning = false;
                Killed?.Invoke();
            });
            thread.Start();
        }

        public void AddJob(Action action,JobPriority priority)
        {
            _jobs.GetOrAdd(priority, (x) => new ConcurrentStack<Action>()).Push(action);
            RunInternal();
        }

        public void Kill()
        {
            _isRunning = false;
        }
    }
}