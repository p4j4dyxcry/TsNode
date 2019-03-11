using System.ComponentModel;

namespace TsNode.Interface
{
    public interface IConnectionViewModel : INotifyPropertyChanged , ISelectable
    {
        IPlugViewModel SourcePlug { get; set; }
        IPlugViewModel DestPlug { get; set; }
    }
}
