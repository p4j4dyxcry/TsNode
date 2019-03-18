using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Livet;
using Reactive.Bindings;
using TsGui.Operation;
using TsNode.Controls;
using TsNode.Interface;
using TsProperty;

namespace WpfApp1
{
    public class ConnectionCreator
    {
        public ReactiveCommand<CompletedCreateConnectionEventArgs> ConnectCommand { get; }
        public ReactiveCommand<StartCreateConnectionEventArgs> StartNewConnectionCommand { get; }

        public ConnectionCreator(ObservableCollection<IConnectionViewModel> connecions , IOperationController @operator)
        {
            var removeOperation = Operation.Empty;

            ConnectCommand = new ReactiveCommand<CompletedCreateConnectionEventArgs>();
            ConnectCommand.Subscribe((e) =>
            {
                var operation = make_remove_duplication_plugs_operation(new[]
                    {
                        e.ConnectionViewModel.DestPlug,
                        e.ConnectionViewModel.SourcePlug
                    })
                    .CombineOperations(connecions.ToAddOperation(e.ConnectionViewModel))
                    .ToCompositeOperation();

                operation.Name = "コネクション接続";

                if (removeOperation.IsEmpty())
                    operation.ExecuteTo(@operator);
                else
                    operation.ExecuteAndCombineTop(@operator);
            });

            StartNewConnectionCommand = new ReactiveCommand<StartCreateConnectionEventArgs>();
            StartNewConnectionCommand.Subscribe((e) =>
            {
                removeOperation = make_remove_duplication_plugs_operation(e.SenderPlugs);

                if (removeOperation.IsNotEmpty())
                    removeOperation.ExecuteTo(@operator);

            });

            IOperation make_remove_duplication_plugs_operation(IPlugViewModel[] plugs)
            {
                var removeConnections = connecions.Where(x => plugs?.Contains(x.DestPlug) is true ||
                                                               plugs?.Contains(x.SourcePlug) is true).ToArray();

                if (removeConnections.Length is 0)
                    return Operation.Empty;

                var operation = connecions
                    .ToRemoveRangeOperation(removeConnections);
                operation.Name = "コノクションの削除";
                return operation;
            }
        }
    }

    public sealed class NetworkViewModel : ViewModel
    {
        public ObservableCollection<INodeViewModel> Nodes { get; set; }

        public ObservableCollection<IConnectionViewModel> Connections { get; set; }

        public ICommand ConnectCommand => _connectionCreator.ConnectCommand;
        public ICommand StartNewConnectionCommand =>_connectionCreator.StartNewConnectionCommand;
        public ReactiveCommand<SelectionChangedEventArgs> SelectionChangedCommand { get; }
        public ReactiveCommand<CompletedMoveNodeEventArgs> NodeMoveCommand { get; }

        private ObservableCollection<IProperty> _properties;

        public ObservableCollection<IProperty> Properties
        {
            get => _properties;
            set => RaisePropertyChangedIfSet(ref _properties, value);
        }

        private readonly IOperationController _operationController;
        private ConnectionCreator _connectionCreator;

        public NetworkViewModel(IOperationController operationController)
        {
            _operationController = operationController;

            var node1 = new NodeViewModel(_operationController) { X = 256 };
            var node2 = new NodeViewModel(_operationController) { Y = 256 };
            Nodes = new ObservableCollection<INodeViewModel>()
            {
                node1,
                node2
            };
            node1.InputPlugs.Add(new PlugViewModel(_operationController));
            node1.InputPlugs.Add(new PlugViewModel(_operationController));
            node1.OutputPlugs.Add(new PlugViewModel(_operationController));

            node2.InputPlugs.Add(new PlugViewModel(_operationController));
            node2.InputPlugs.Add(new PlugViewModel(_operationController));
            node2.OutputPlugs.Add(new PlugViewModel(_operationController));

            Connections = new ObservableCollection<IConnectionViewModel>();
            _connectionCreator = new ConnectionCreator(Connections,_operationController);

            NodeMoveCommand = new ReactiveCommand<CompletedMoveNodeEventArgs>();
            NodeMoveCommand.Subscribe(e =>
            {
                if (e.InitialNodePoints.Count is 0)
                    return;

                var operationBuilder = new OperationBuilder();
                var operation = operationBuilder.MakeFromAction(x =>
                    {
                        foreach (var keyValuePair in x)
                        {
                            keyValuePair.Key.X = keyValuePair.Value.X;
                            keyValuePair.Key.Y = keyValuePair.Value.Y;
                        }
                    },e.CompletedNodePoints, e.InitialNodePoints)
                    .Name("ノード移動")
                    .Build();
                _operationController.Push(operation);
            });

            SelectionChangedCommand = new ReactiveCommand<SelectionChangedEventArgs>();
            SelectionChangedCommand.Subscribe(e =>
            {
                bool[] selected = e.ChangedItems.Select(x => x.IsSelected).ToArray();

                var operationBuilder = new OperationBuilder();
                operationBuilder.MakeFromAction(
                    () =>
                    {
                        foreach(var i in Enumerable.Range(0, selected.Length))
                            e.ChangedItems[i].IsSelected = selected[i];
                    },
                    () =>
                    {
                        foreach (var i in Enumerable.Range(0, selected.Length))
                            e.ChangedItems[i].IsSelected = !selected[i];
                    })
                    .PostEvent(() => GenerateProperties(Nodes.Where(x => x.IsSelected)))
                    .Name("選択変更")
                    .Build()
                    .PushTo(_operationController);
                GenerateProperties(Nodes.Where(x => x.IsSelected));
            });
        }

        public void GenerateProperties(IEnumerable<ISelectable> items)
        {
            var selectables = items as ISelectable[] ?? items.ToArray();
            if (selectables.Any() is false)
            {
                Properties = Enumerable.Empty<IProperty>().ToObservableCollection();
                return;
            }

            Properties = new ReflectionPropertyBuilder(selectables.First())
                .GenerateProperties()
                .OperationController(_operationController)
                .Build()
                .ToObservableCollection();
        }
    }

}
