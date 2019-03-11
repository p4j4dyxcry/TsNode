using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Livet;
using TsNode.Interface;

namespace WpfApp1
{
    public class NodeViewModel : NotificationObject , INodeViewModel
    {
        private double _x ;

        public double X
        {
            get => _x;
            set => RaisePropertyChangedIfSet(ref _x, value);
        }

        private double _y ;

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

        public ObservableCollection<IPlugViewModel> InputPlugs { get; set; } = new ObservableCollection<IPlugViewModel>();
        public ObservableCollection<IPlugViewModel> OutputPlugs { get; set; } = new ObservableCollection<IPlugViewModel>();

        public IEnumerable<IPlugViewModel> GetInputPlugs()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPlugViewModel> GetOutputPlugs()
        {
            throw new NotImplementedException();
        }


        public NodeViewModel()
        {
            InputPlugs.Add(new PlugViewModel());
            OutputPlugs.Add(new PlugViewModel());
        }

        public bool TryConnect(ConnectInfo connectInfo)
        {
            return false;
        }
    }
}
