using TsNode.Interface;

namespace TsNode.Preset
{
    /// <summary>
    /// 最低限実装のConnectionVieModel
    /// </summary>
    public class PresetConnectionViewModel : PresetNotification, IConnectionDataContext
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set => RaisePropertyChangedIfSet(ref _isSelected, value);
        }
        private IPlugDataContext _sourcePlug;

        public IPlugDataContext SourcePlug
        {
            get => _sourcePlug;
            set => RaisePropertyChangedIfSet(ref _sourcePlug, value);
        }

        private IPlugDataContext _destPlug;
        public IPlugDataContext DestPlug
        {
            get => _destPlug;
            set => RaisePropertyChangedIfSet(ref _destPlug, value);
        }
    }
}