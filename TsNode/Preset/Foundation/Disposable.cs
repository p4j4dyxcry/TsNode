using System;

namespace TsNode.Preset.Foundation
{
    internal static class Disposable
    {
        private class EmptyDisposable : IDisposable
        {
            public void Dispose()
            {
                // not impl
            }
        }
        public static IDisposable Empty { get; } = new EmptyDisposable();
        
    }
}