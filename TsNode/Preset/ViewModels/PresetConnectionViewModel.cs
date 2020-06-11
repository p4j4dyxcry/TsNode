using TsNode.Interface;
using TsNode.Preset.Foundation;
using TsNode.Preset.Models;

namespace TsNode.Preset.ViewModels
{
    /// <summary>
    /// 実装サンプル
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

        public PresetConnectionViewModel()
        {
            
        }

        private class Converter : ISimpleValueConverter
        {
            public static Converter Instance { get; } = new Converter();
            public object Convert(object sender)
            {
                return PlugViewModelService.GetOrCreate(sender as PresetPlug);
            }

            public object ConvertBack(object sender)
            {
                return PlugViewModelService.ContextToModel(sender as IPlugDataContext);
            }
        }

        public PresetConnectionViewModel(PresetConnection connection)
        {
            SourcePlug = PlugViewModelService.GetOrCreate(connection.SourcePlug);
            DestPlug   = PlugViewModelService.GetOrCreate(connection.DestPlug);
            this.BindModel(connection,BindMode.TwoWay, Converter.Instance);
        }
    }
}