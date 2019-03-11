using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Livet;
using Reactive.Bindings.Extensions;
using TsGui.Operation;
using TsNode.Interface;
using TsNode.Preset;

namespace WpfApp1
{
    public class PlugViewModel : PresentPlugViewModel
    {
        private IOperationController _operationController;
        public PlugViewModel(IOperationController controller)
        {
            _operationController = controller;
        }

        public override IConnectionViewModel StartConnectionOverride()
        {
            return new ConnectionViewModel(_operationController);
        }
    }
}