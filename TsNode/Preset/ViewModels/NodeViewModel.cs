using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using TsNode.Controls;
using TsNode.Interface;
using TsNode.Preset.Models;
using static System.Windows.Media.ColorConverter;

namespace TsNode.Preset
{
    /// <summary>
    /// 組み込みノードビューモデル
    /// </summary>
    public class PresetNodeViewModel : PresetNotification, IConnectableNodeContext
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

        private Color _headerColor = (Color)ColorConverter.ConvertFromString("#2c3e50");
        public Color HeaderColor
        {
            get => _headerColor;
            set => RaisePropertyChangedIfSet(ref _headerColor, value);
        }
        
        private Color _backGroundColor = (Color)ColorConverter.ConvertFromString("#bdc3c7");
        public Color BackGroundColor
        {
            get => _backGroundColor;
            set => RaisePropertyChangedIfSet(ref _backGroundColor, value);
        }
        
        private Color _headerTextColor = (Color)ColorConverter.ConvertFromString("#ecf0f1");
        public Color HeaderTextColor
        {
            get => _headerTextColor;
            set => RaisePropertyChangedIfSet(ref _headerTextColor, value);
        }
        
        private string _name = "Preset Node";
        public string Name
        {
            get => _name;
            set => RaisePropertyChangedIfSet(ref _name, value);
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

        public PresetNodeViewModel()
        {
            
        }

        public PresetNodeViewModel(PresetNode model)
        {
            Name = model.Name;
            X = model.X;
            Y = model.Y;
            HeaderColor = model.HeaderColor;
            HeaderTextColor = model.HeaderTextColor;
            BackGroundColor = model.BackGroundColor;
            InputPlugs = model.InputPlugs.Mapping(PlugViewModelService.GetOrCreate);
            OutputPlugs = model.OutputPlugs.Mapping(PlugViewModelService.GetOrCreate);
            
            this.BindModel(model);
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
