using System;
using System.Collections.Generic;
using TsNode.Controls;

namespace TsNode.Interface
{
    public interface INodeControl : ISelectable
    {
        double X { get; set; }

        double Y { get; set; }
        
        bool CanMovable { get; }
        
        bool IsMouseOver { get; }
        
        object DataContext { get; }

        double ActualWidth { get; }
        
        double ActualHeight { get; }
        
        event Action<object, UpdateNodePointArgs> UpdatePoints;

        IEnumerable<IPlugControl> GetOutputPlugs();
        
        IEnumerable<IPlugControl> GetInputPlugs();
    }
}