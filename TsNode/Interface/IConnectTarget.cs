using TsNode.Controls;

namespace TsNode.Interface
{
    public struct ConnectInfo
    {
        public IPlugViewModel Sender { get; set; }
        public SourcePlugType SenderType { get; set; }
        public IConnectionViewModel Connection { get; set; }
    }

    public interface IConnectTarget
    {
        bool TryConnect(ConnectInfo connectInfo);
    }
}
