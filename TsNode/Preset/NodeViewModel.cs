using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TsNode.Controls;
using TsNode.Interface;

namespace TsNode.Preset
{
    /// <summary>
    /// 最低限実装のNodeVieModel
    /// </summary>
    public class PresetNodeViewModel : PresetNotification, INodeDataContext
    {
        private double _x;

        public double X
        {
            get => _x;
            set => RaisePropertyChangedIfSet(ref _x, value);
        }

        private double _y;

        public double Y
        {
            get => _y;
            set => RaisePropertyChangedIfSet(ref _y, value);
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set => RaisePropertyChangedIfSet(ref _isSelected, value);
        }

        public ObservableCollection<IPlugDataContext> InputPlugs { get; set; } = new ObservableCollection<IPlugDataContext>();
        public ObservableCollection<IPlugDataContext> OutputPlugs { get; set; } = new ObservableCollection<IPlugDataContext>();

        public IEnumerable<IPlugDataContext> GetInputPlugs()
        {
            return InputPlugs;
        }

        public IEnumerable<IPlugDataContext> GetOutputPlugs()
        {
            return OutputPlugs;
        }

        public bool TryConnect(ConnectInfo connectInfo)
        {
            if (InputPlugs.Contains(connectInfo.Sender))
                return false;

            if (OutputPlugs.Contains(connectInfo.Sender))
                return false;

            if (connectInfo.SenderType == SourcePlugType.Output)
                return InputPlugs.Any(x => x.TryConnect(connectInfo));
            return OutputPlugs.Any(x => x.TryConnect(connectInfo));
        }
    }
}
