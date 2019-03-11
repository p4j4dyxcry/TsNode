using TsNode.Interface;

namespace TsNode.Preset
{
    public class PresetConnectionViewModel : PresetNotification, IConnectionViewModel
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set => RaisePropertyChangedIfSet(ref _isSelected, value);
        }
        private IPlugViewModel _sourcePlug;

        public IPlugViewModel SourcePlug
        {
            get => _sourcePlug;
            set => RaisePropertyChangedIfSet(ref _sourcePlug, value);
        }

        private IPlugViewModel _destPlug;
        public IPlugViewModel DestPlug
        {
            get => _destPlug;
            set => RaisePropertyChangedIfSet(ref _destPlug, value);
        }
    }
}