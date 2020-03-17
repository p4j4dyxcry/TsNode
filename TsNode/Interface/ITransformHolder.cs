using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TsNode.Interface
{
    public interface ITransformHolder
    {
        ScaleTransform ScaleMatrix { get; }

        TranslateTransform TranslateMatrix { get; }
    }
}
