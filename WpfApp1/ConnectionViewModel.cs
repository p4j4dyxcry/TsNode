using System.Reactive.Linq;
using Reactive.Bindings;
using TsCore.Operation;
using TsNode.Preset;

namespace WpfApp1
{
    public class ConnectionViewModel : PresetConnectionViewModel
    {
        public int Name { get; set; }

        public ConnectionViewModel( IOperationController controller )
        {

        }
    }
}
