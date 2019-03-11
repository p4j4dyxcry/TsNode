using Livet;
using TsNode.Interface;

namespace WpfApp1
{
    public class ConnectionViewModel : NotificationObject, IConnectionViewModel
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => RaisePropertyChangedIfSet(ref _isSelected, value);
        }

        public ConnectionViewModel()
        {

        }

        public IPlugViewModel SourcePlug { get; set; }
        public IPlugViewModel DestPlug { get; set; }
    }
}
