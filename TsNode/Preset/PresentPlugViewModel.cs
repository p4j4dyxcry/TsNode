using TsNode.Interface;

namespace TsNode.Preset
{
    /// <summary>
    /// 実装サンプル
    /// </summary>
    public class PresentPlugViewModel : PresetNotification , IPlugDataContext
    {
        public virtual IConnectionDataContext StartConnectionOverride()
        {
            return new PresetConnectionViewModel();
        }

        public IConnectionDataContext StartConnection()
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
