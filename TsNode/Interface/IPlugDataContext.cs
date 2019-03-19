using System.ComponentModel;

namespace TsNode.Interface
{
    //! プラグに設定するDataContextはこのinterfaceを利用すること
    public interface IPlugDataContext : INotifyPropertyChanged , IConnectTarget
    {
        IConnectionDataContext StartConnection();
    }
}
