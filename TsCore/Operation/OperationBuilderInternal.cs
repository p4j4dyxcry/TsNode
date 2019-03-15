using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TsGui.Operation.Internal;

namespace TsGui.Operation
{
    public partial class OperationBuilder
    {
        private class EventBinder
        {
            private readonly ICollection<Action> _postEvents = new HashSet<Action>();
            private readonly ICollection<Action> _prevEvents = new HashSet<Action>();

            public void AddPostEvent(Action action)
            {
                Debug.Assert(action != null, ErrorMessages.NotNull);
                _postEvents.Add(action);
            }

            public void AddPreEvent(Action action)
            {
                Debug.Assert(action != null, ErrorMessages.NotNull);
                _prevEvents.Add(action);
            }

            public IOperation BindEvents(IOperation operation)
            {
                Debug.Assert(operation != null, ErrorMessages.NotNull);
                if (_prevEvents.Any())
                    operation = operation.AddPreEvent(() =>
                    {
                        foreach (var e in _prevEvents)
                            e.Invoke();
                    });

                if (_postEvents.Any())
                    operation = operation.AddPostEvent(() =>
                    {
                        foreach (var e in _postEvents)
                            e.Invoke();
                    });

                return operation;
            }
        }

        private class OperationBuilderFromValues<T> :  IBuilderFromValues<T> 
        {
            private readonly Action<T> _function;
            private T _prev;
            private T _new;
            private IMergeJudge _mergeJudge;
            private readonly EventBinder _eventEventBinder = new EventBinder();
            private bool _canBuild;
            private string _name;

            public OperationBuilderFromValues(Action<T> function)
            {
                Debug.Assert(function != null, ErrorMessages.NotNull);
                _function = function;
            }

            public IBuilderFromValues<T> Values(T newValue, T prevValue)
            {
                _canBuild = true;
                _prev = prevValue;
                _new = newValue;
                return this;
            }

            public IBuilderFromValues<T> Throttle(object key , TimeSpan timeSpan)
            {
                Debug.Assert(key != null , ErrorMessages.NotNull);
                _mergeJudge = new ThrottleMergeJudge<int>(key.GetHashCode(),timeSpan);
                return this;
            }

            public IOperationBuilder Name(string name)
            {
                _name = name;
                return this;
            }

            public IOperationBuilder PostEvent(Action action)
            {
                Debug.Assert(action != null, ErrorMessages.NotNull);
                _eventEventBinder.AddPostEvent(action);
                return this;
            }

            public IOperationBuilder PrevEvent(Action action)
            {
                Debug.Assert(action != null, ErrorMessages.NotNull);
                _eventEventBinder.AddPreEvent(action);
                return this;
            }

            public IOperation Build()
            {
                Debug.Assert(_canBuild,ErrorMessages.InvalidOperation);
                var operation = new MergeableOperation<T>(_function, _new, _prev, _mergeJudge)
                {
                    Name = _name
                };
                return _eventEventBinder.BindEvents(operation);
            }
        }

        private class BuilderFromNewOperationValue<T> : IBuilderFromNewValue<T>
        {
            private readonly object _sender;
            private readonly string _propertyName;
            private T _newValue;
            private TimeSpan _throttleTimeSpan;
            private readonly EventBinder _eventEventBinder = new EventBinder();
            private bool _canBuild;
            private string _name;

            public BuilderFromNewOperationValue(object sender, string propertyName)
            {
                Debug.Assert(sender != null, ErrorMessages.NotNull);
                Debug.Assert(propertyName != null, ErrorMessages.NotNull);
                Debug.Assert(propertyName != string.Empty, ErrorMessages.InvalidOperation);

                _sender = sender;
                _propertyName = propertyName;
                _throttleTimeSpan = Operation.DefaultMergeSpan;
            }

            public IBuilderFromNewValue<T> NewValue(T newValue)
            {
                _canBuild = true;
                _newValue = newValue;
                return this;
            }

            public IBuilderFromNewValue<T> Throttle(TimeSpan timeSpan)
            {
                _throttleTimeSpan = timeSpan;
                return this;
            }

            public IOperationBuilder Name(string name)
            {
                _name = name;
                return this;
            }

            public IOperationBuilder PostEvent(Action action)
            {
                Debug.Assert(action != null, ErrorMessages.NotNull);
                _eventEventBinder.AddPostEvent(action);
                return this;
            }

            public IOperationBuilder PrevEvent(Action action)
            {
                Debug.Assert(action != null, ErrorMessages.NotNull);
                _eventEventBinder.AddPreEvent(action);                
                return this;
            }

            public IOperation Build()
            {
                Debug.Assert(_canBuild, ErrorMessages.InvalidOperation);

                var operation = _sender.GenerateSetOperation(_propertyName, _newValue, _throttleTimeSpan);
                operation.Name = _name;
                return _eventEventBinder.BindEvents(operation);
            }
        }

        private class CollectionOperationBuilder<T> : ICollectionOperationBuilder<T>
        {
            private readonly IList<T> _list;

            public CollectionOperationBuilder(IList<T> list)
            {
                Debug.Assert(list != null, ErrorMessages.NotNull);
                _list = list;
            }

            public IOperation BuildAddOperation(T addValue)
            {
                return _list.ToAddOperation(addValue);
            }

            public IOperation BuildRemoveOperation(T removeValue)
            {
                return _list.ToRemoveOperation(removeValue);
            }

            public IOperation BuildAddRangeOperation(IEnumerable<T> addValues)
            {
                Debug.Assert(addValues != null, ErrorMessages.NotNull);
                return _list.ToAddRangeOperation(addValues);
            }

            public IOperation BuildRemoveRangeOperation(IEnumerable<T> removeValues)
            {
                Debug.Assert(removeValues != null, ErrorMessages.NotNull);
                return _list.ToRemoveRangeOperation(removeValues);
            }

            public IOperation BuildClearOperation()
            {
                return _list.ToClearOperation();
            }
        }

        private class CollectionOperationCustomizer<T> : ICollectionOperationCustomizer<T>
        {
            private readonly ICollection<Action<T>> _addEvents    = new HashSet<Action<T>>();
            private readonly ICollection<Action> _addedEvents     = new HashSet<Action>();
            private readonly ICollection<Action<T>> _removeEvents = new HashSet<Action<T>>();
            private readonly ICollection<Action> _removedEvents   = new HashSet<Action>();
            private readonly ICollection<Action> _clearEvents       = new HashSet<Action>();
            private readonly ICollection<Action> _rollbackEvents  = new HashSet<Action>();
            public ICollectionOperationCustomizer<T> RegisterAdd(Action<T> value)
            {
                _addEvents.Add(value);
                return this;
            }

            public ICollectionOperationCustomizer<T> RegisterRemove(Action<T> function)
            {
                Debug.Assert(function != null, ErrorMessages.NotNull);
                _removeEvents.Add(function);
                return this;
            }

            public ICollectionOperationCustomizer<T> RegisterAdd(Action function)
            {
                Debug.Assert(function != null, ErrorMessages.NotNull);
                _addedEvents.Add(function);
                return this;
            }

            public ICollectionOperationCustomizer<T> RegisterRemove(Action function)
            {
                Debug.Assert(function != null, ErrorMessages.NotNull);
                _removedEvents.Add(function);
                return this;
            }

            public ICollectionOperationCustomizer<T> RegisterClear(Action function)
            {
                Debug.Assert(function != null, ErrorMessages.NotNull);
                _clearEvents.Add(function);
                return this;

            }

            public ICollectionOperationCustomizer<T> RegisterRollback(Action function)
            {
                Debug.Assert(function != null, ErrorMessages.NotNull);
                _rollbackEvents.Add(function);
                return this;
            }

            public IOperation BuildAddOperation(T addValue)
            {
                Debug.Assert(_addEvents.Any() && _removeEvents.Any(), ErrorMessages.InvalidOperation);

                return new DelegateOperation(
                    MakeInvokeListAction(_addEvents,_addedEvents,addValue),
                    MakeInvokeListAction(_removeEvents, _removedEvents, addValue));
            }

            public IOperation BuildRemoveOperation(T removeValue)
            {
                Debug.Assert(_addEvents.Any() && _removeEvents.Any(), ErrorMessages.InvalidOperation);

                return new DelegateOperation(
                    MakeInvokeListAction(_removeEvents, _removedEvents, removeValue),
                    MakeInvokeListAction(_addEvents, _addedEvents, removeValue));
            }

            public IOperation BuildAddRangeOperation(IEnumerable<T> addValues)
            {
                Debug.Assert(addValues != null, ErrorMessages.NotNull);
                return addValues.Select(BuildAddOperation).ToCompositeOperation();
            }

            public IOperation BuildRemoveRangeOperation(IEnumerable<T> removeValues)
            {
                Debug.Assert(removeValues != null, ErrorMessages.NotNull);
                return removeValues.Select(BuildRemoveOperation).ToCompositeOperation();
            }

            public IOperation BuildClearOperation()
            {
                Debug.Assert(_clearEvents.Any() && _rollbackEvents.Any(), ErrorMessages.InvalidOperation);

                return new DelegateOperation(
                    () =>
                    {
                        foreach (var action in _clearEvents)
                            action.Invoke();

                    },
                    () =>
                    {
                        foreach (var action in _rollbackEvents)
                            action.Invoke();
                    });
            }

            private Action MakeInvokeListAction(IEnumerable<Action<T>> actions, IEnumerable<Action> postActions, T value)
            {
                return () =>
                {
                    foreach (var action in actions)
                        action.Invoke(value);

                    foreach (var action in postActions)
                        action.Invoke();

                };
            }
        }

        private class Builder : IOperationBuilder
        {
            private IOperation _operation;

            public Builder(IOperation operation)
            {
                Debug.Assert(operation != null, ErrorMessages.NotNull);

                _operation = operation;
            }

            public IOperationBuilder Name(string name)
            {
                _operation.Name = name;
                return this;
            }

            public IOperationBuilder PostEvent(Action action)
            {
                Debug.Assert(action != null, ErrorMessages.NotNull);
                _operation = _operation.AddPostEvent(action);
                return this;
            }

            public IOperationBuilder PrevEvent(Action action)
            {
                Debug.Assert(action != null, ErrorMessages.NotNull);
                _operation = _operation.AddPreEvent(action);
                return this;
            }

            public IOperation Build()
            {
                return _operation;
            }
        }

        private class MergeableBuilder : Builder , IMergeableOperationBuilder
        {
            private readonly MergeableOperation _mergeableOperation;

            public MergeableBuilder(MergeableOperation operation) : base(operation)
            {
                _mergeableOperation = operation;
            }

            public IMergeableOperationBuilder SetActionName(string executeAction, string rollbackAction)
            {
                _mergeableOperation.SetActionName(executeAction,rollbackAction);
                return this;
            }
        }

        private class MergeableBuilder<T> : Builder, IMergeableOperationBuilder
        {
            private readonly MergeableOperation<T> _mergeableOperation;

            public MergeableBuilder(MergeableOperation<T> operation) : base(operation)
            {
                _mergeableOperation = operation;
            }

            public IMergeableOperationBuilder SetActionName(string executeAction, string rollbackAction)
            {
                _mergeableOperation.Name = executeAction;
                return this;
            }
        }
    }
}
