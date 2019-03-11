using System.Collections.Generic;
using System.Linq;
using TsNode.Interface;

namespace TsNode.Preset
{
    public class PresentPlugViewModel : PresetNotification , IPlugViewModel
    {
        public IConnectionViewModel StartConnection()
        {
            return new PresetConnectionViewModel();
        }

        public bool TryConnect(ConnectInfo connectInfo)
        {
            if (connectInfo.Connection.SourcePlug is null)
                connectInfo.Connection.SourcePlug = this;
            else if(connectInfo.Connection.DestPlug is null)
                connectInfo.Connection.DestPlug = this;

             return true;
        }
    }
}
