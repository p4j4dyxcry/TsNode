using TsNode.Controls.Connection;
using TsNode.Controls.Node;

namespace TsNode.Interface
{
    public struct SelectInfo
    {
        public NodeControl[] AllNodes { get; }
        public NodeControl[] NewSelectNodes { get; }

        public SelectInfo( NodeControl[] nodes , NodeControl[] newSelectNodes )
        {
            AllNodes = nodes;
            NewSelectNodes = newSelectNodes;
        }
    }

    public interface IControlSelector
    {
        void OnSelect(SelectInfo selectInfo);
    }
}
