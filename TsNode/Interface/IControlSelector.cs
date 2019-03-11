using TsNode.Controls.Connection;
using TsNode.Controls.Node;

namespace TsNode.Interface
{
    public struct SelectInfo
    {
        public ISelectable[] AllNodes { get; }
        public ISelectable[] NewSelectNodes { get; }

        public ISelectable[] Connections { get; }
        public ISelectable[] NewConnections { get; }

        public SelectInfo(ISelectable[] nodes , ISelectable[] newSelectNodes , ISelectable[] connections , ISelectable[] newSelectConnections )
        {
            AllNodes = nodes;
            NewSelectNodes = newSelectNodes;
            Connections = connections;
            NewConnections = newSelectConnections;
        }
    }

    public interface IControlSelector
    {
        void OnSelect(SelectInfo selectInfo);
    }
}
