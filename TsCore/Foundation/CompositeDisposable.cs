using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsGui.Foundation
{
    public class CompositeDisposable : List<IDisposable> , IDisposable
    {
        public CompositeDisposable(IEnumerable<IDisposable> source)
            : base(source) { }

        public void RemoveAndDispose(IDisposable source)
        {
            if (Contains(source))
            {
                Remove(source);
                source.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var item in this)
                item.Dispose();
        }
    }
}