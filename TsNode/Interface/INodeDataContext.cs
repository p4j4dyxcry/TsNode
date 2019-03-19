using System.Collections.Generic;
using System.ComponentModel;

namespace TsNode.Interface
{
    //! ノードに設定するDataContextはこのinterfaceを利用すること
    public interface INodeDataContext : INotifyPropertyChanged , ISelectable , IConnectTarget
    {
        double X { get; set; }
        double Y { get; set; }

        IEnumerable<IPlugDataContext> GetInputPlugs();
        IEnumerable<IPlugDataContext> GetOutputPlugs();
    }
}
