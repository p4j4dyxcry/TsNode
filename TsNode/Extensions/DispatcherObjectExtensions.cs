using System;
using System.Windows.Threading;

namespace TsNode.Extensions
{
    internal static class DispatcherObjectExtensions
    {
        public static void BeginInvoke(this DispatcherObject _this, Action action , DispatcherPriority priority = DispatcherPriority.Normal)
        {
            _this?.Dispatcher?.BeginInvoke(priority, action);
        }
    }
}