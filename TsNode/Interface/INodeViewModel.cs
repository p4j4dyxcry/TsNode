using System.Collections.Generic;
using System.ComponentModel;

namespace TsNode.Interface
{
    public interface INodeViewModel : INotifyPropertyChanged , ISelectable , IConnectTarget
    {
        double X { get; set; }
        double Y { get; set; }

        IEnumerable<IPlugViewModel> GetInputPlugs();
        IEnumerable<IPlugViewModel> GetOutputPlugs();
    }
}
