using System;
using System.Diagnostics;

namespace TsCore.Foundation
{
    public class SpeedChecker : IDisposable
    {
        public class SpeedCheckerArgs : EventArgs
        {
            public long ElapsedMilliseconds { get; set; }
            public string Message { get; set; }
        }
        public static event Action<object, SpeedCheckerArgs> Checked;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly string _message;
        public SpeedChecker(string message)
        {
            _message = message;
            _stopwatch.Start();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            Checked?.Invoke(this,new SpeedCheckerArgs()
            {
                ElapsedMilliseconds = _stopwatch.ElapsedMilliseconds,
                Message = _message,
            });
        }
    }
}