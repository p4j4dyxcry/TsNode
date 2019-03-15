using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace TsGui.Thread
{
    public class TaskCanceler : IDisposable
    {
        private static readonly ConcurrentDictionary<object, CancellationTokenSource> CancellationTokens
            = new ConcurrentDictionary<object, CancellationTokenSource>();

        /// <summary>
        /// タスクが実行中かどうか判断します。
        /// </summary>
        /// <param name="uniqueKey"></param>
        /// <returns></returns>
        public bool IsRunning(object uniqueKey)
        {
            if (CancellationTokens.ContainsKey(uniqueKey) is false)
                return false;

            if (CancellationTokens[uniqueKey] is null)
                return false;

            return true;
        }

        /// <summary>
        /// 実行をキャンセルします
        /// </summary>
        /// <param name="key"></param>
        public void Cancel(object key)
        {
            if (CancellationTokens.ContainsKey(key) is false)
                return ;

            var token = CancellationTokens[key];
            token?.Cancel();
            CancellationTokens[key] = null;
        }

        /// <summary>
        /// 実行中のすべてのタスクをキャンセルします
        /// </summary>
        public void CancelAll()
        {
            foreach (var keyValue in CancellationTokens)
            {
                var token = CancellationTokens[keyValue];
                token?.Cancel();
                CancellationTokens[keyValue] = null;
            }
        }

        /// <summary>
        /// 最後の実行された非同期メソッド命令をキャンセルし、新しく実行します
        /// </summary>
        /// <param name="function"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="canceled"></param>
        /// <returns></returns>
        public async Task CancelLastCallAndRun(object uniqueKey, Action<CancellationToken> function,  Action canceled = null)
        {
            CancellationTokenSource tokenSource; 

            if(uniqueKey is null)
                tokenSource = new CancellationTokenSource();
            else
            {
                tokenSource = CancellationTokens.GetOrAdd(uniqueKey, (x) => new CancellationTokenSource());
                tokenSource?.Cancel();
                tokenSource = CancellationTokens[uniqueKey] = new CancellationTokenSource();
            }
            
            using (tokenSource)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        function.Invoke(tokenSource.Token);
                        if (uniqueKey != null)
                            CancellationTokens[uniqueKey] = null;
                    }
                    catch (OperationCanceledException)
                    {
                        canceled?.Invoke();
                    }

                },tokenSource.Token);
            }
        }

        private bool _disposed; 
        public void Dispose()
        {
            if(_disposed)
                return;
            _disposed = true;
            foreach (var token in CancellationTokens)
                token.Value?.Dispose();
        }
    }
}

