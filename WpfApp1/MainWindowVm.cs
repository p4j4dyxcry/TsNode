using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Livet;
using Livet.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TsGui.Operation;

namespace WpfApp1
{
    // Undo / Redo Command ViewModel
    public class OperationVm  : NotificationObject
    {
        public string Name { get; }
        public bool IsRedo { get; }
        public ICommand GotoCommand { get; }

        public OperationVm(IOperation operation , IOperationController controller )
        {
            Name = operation.Name;
            GotoCommand = new ViewModelCommand(()=>controller.MoveTo(operation));
            IsRedo = controller.Operations.All(x=> x!= operation);
        }
    }

    public class MainWindowVm : ViewModel
    {
        public NetworkViewModel NetworkVm { get; set; }
        public IOperationController OperationController { get; } = new OperationController(1024);

        public ReactiveCommand UndoCommand { get; }
        public ReactiveCommand RedoCommand { get; }

        // Undo / Redo バッファ
        public ObservableCollection<OperationVm> Operations { get; set; }

        public MainWindowVm()
        {
            NetworkVm = new NetworkViewModel(OperationController);

            // Undo Command
            UndoCommand =
                OperationController
                    .StackChangedAsObservable()
                    .Select(x => OperationController.CanUndo)
                    .ToReactiveCommand();
            UndoCommand.Subscribe(() => OperationController.Undo());

            //! Redo Command
            RedoCommand =
                OperationController
                    .StackChangedAsObservable()
                    .Select(x => OperationController.CanRedo)
                    .ToReactiveCommand();
            RedoCommand.Subscribe(() => OperationController.Redo());

            //! Undo / Redoが行われたときに Undo / Redo ViewModelを生成する
            OperationController.StackChangedAsObservable().Subscribe(_=>
            {
                Operations = OperationController
                    .Operations.Concat(OperationController.RollForwardTargets)
                    .Select(x => new OperationVm(x, OperationController))
                    .ToObservableCollection();

                RaisePropertyChanged(nameof(Operations));
            }).AddTo(CompositeDisposable);
        }
    }

    // OperationSystemをRxにつなげるための拡張機能
    public static class OperationControllerEx
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> self)
        {
            return new ObservableCollection<T>(self);
        }

        // IOperationController をIObservable<Unit>に変換
        public static IObservable<Unit> StackChangedAsObservable(this IOperationController self)
        {
            return Observable
                .FromEventPattern<Action<object, OperationStackChangedEventArgs>, OperationStackChangedEventArgs>(
                    h => self.StackChanged += h, h => self.StackChanged -= h)
                .ToUnit()
                .StartWithDefault();
        }

        public static IObservable<Unit> StartWithDefault(this IObservable<Unit> source)
        {
            bool isFirst = true;
            return Observable.Defer(() =>
            {
                bool flag = isFirst;
                isFirst = false;
                return flag ? source.StartWith(Unit.Default) : source;
            });
        }
    }
}
