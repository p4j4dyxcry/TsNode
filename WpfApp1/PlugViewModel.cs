using System.Collections.Generic;
using Livet;
using TsNode.Interface;
using TsNode.Preset;

namespace WpfApp1
{
    public class PlugViewModel : ViewModel , IPlugViewModel
    {
        public bool TryConnect(ConnectInfo info)
        {
            return false;
        }

        public IConnectionViewModel StartConnection()
        {
            return new PresetConnectionViewModel();
        }
    }
}