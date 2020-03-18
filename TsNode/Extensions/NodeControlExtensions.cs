using System.Windows;
using TsNode.Interface;

namespace TsNode.Extensions
{
    public static class NodeControlExtensions
    {
        public static Rect ToRect(this INodeControl node)
        {
            return new Rect(node.X , node.Y , node.ActualWidth , node.ActualHeight);
        }
    }
}