using TsNode.Interface;

namespace TsNode.Preset
{
    public class PresentPlugViewModel : PresetNotification , IPlugViewModel
    {
        public string Name { get; set; } = "Plug1";
        public int Test { get; set; }

        public virtual IConnectionViewModel StartConnectionOverride()
        {
            return new PresetConnectionViewModel();
        }

        public IConnectionViewModel StartConnection()
        {
            return StartConnectionOverride();
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
