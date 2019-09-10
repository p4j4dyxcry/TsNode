using TsNode.Preset;

namespace WpfApp2
{
    public class GripNodeVm : PresetNodeViewModel
    {
        private double _w;

        public double W
        {
            get => _w;
            set => RaisePropertyChangedIfSet(ref _w, value);
        }

        private double _h;

        public double H
        {
            get => _h;
            set => RaisePropertyChangedIfSet(ref _h, value);
        }
    }
}