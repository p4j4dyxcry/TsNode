using TsNode.Controls;

namespace TsNode.Interface
{
    public struct ConnectInfo
    {
        public IPlugDataContext Sender { get; set; }
        public SourcePlugType SenderType { get; set; }
        public IConnectionDataContext Connection { get; set; }
    }

    public interface IConnectTarget
    {
        bool TryConnect(ConnectInfo connectInfo);
    }
}
