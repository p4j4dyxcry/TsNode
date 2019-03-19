using System.ComponentModel;

namespace TsNode.Interface
{
    //! コネクションに設定するDataContextはこのinterfaceを利用すること
    public interface IConnectionDataContext : INotifyPropertyChanged , ISelectable
    {
        IPlugDataContext SourcePlug { get; set; }
        IPlugDataContext DestPlug { get; set; }
    }
}
