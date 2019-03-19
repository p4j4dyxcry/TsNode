using System;
using System.Collections.Generic;
using System.Windows;
using TsGui.Operation;
using TsNode.Preset;

namespace WpfApp1
{
    public class NodeViewModel : PresetNodeViewModel
    {
        public NodeViewModel(IOperationController operationController)
        {

        }
    }

    public class NodeViewModel2 : PresetNodeViewModel
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Point Point { get; set; }

        public NodeViewModel2(IOperationController operationController)
        {

        }
    }

    public class NodeViewModel3 : PresetNodeViewModel
    {
        public List<int> IntList { get; set; } = new List<int>() { 100,200,300,400,500};
        public Point[] Points { get; set; } = new Point[3];

        public NodeViewModel3(IOperationController operationController)
        {

        }
    }
}
