using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TsGui.Operation.Internal;

namespace TsGui.Operation
{
    public static class Operation
    {
        private class EmptyOperation : IOperation
        {
            public string Name { get; set; }
            public void RollForward() { }
            public void Rollback() { }
        }

        /// <summary>
        /// 空のオペレーションを取得する
        /// </summary>
        public static IOperation Empty { get; } = new EmptyOperation();

        /// <summary>
        /// 拡張が自動作成するマージコマンドのマージ間隔デフォルト設定
        /// </summary>
        public static TimeSpan DefaultMergeSpan { get; set; } = TimeSpan.MaxValue;
    }

    /// <summary>
    /// オペレーション拡張機能
    /// </summary>
    public static class OperationExtensions
    {
        /// <summary>
        /// コントローラを通してOperationを実行する、マージはこのタイミングで行われる
        /// マージをしたくない場合は直接 IOperationController.Execute()を呼び出す
        /// </summary>
        public static IOperation ExecuteTo(this IOperation _this, IOperationController controller)
        {
            if (_this is IMergeableOperation mergeableOperation)
                _this = mergeableOperation.Merge(controller);
            return controller.Execute(_this);
        }

        /// <summary>
        /// 前回のオペレーションと結合します
        /// </summary>
        public static IOperation ExecuteAndCombineTop(this IOperation _this, IOperationController controller)
        {
            if (controller.Operations.Any())
            {
                var prev = controller.Pop();
                _this.RollForward();
                var newOperation = prev.CombineOperations(_this).ToCompositeOperation();
                newOperation.Name = _this.Name;
                return controller.Push(newOperation);
            }

            return controller.Execute(_this);
        }

        /// <summary>
        /// オペレーションを列挙子として結合する
        /// </summary>
        public static IEnumerable<IOperation> CombineOperations(this IOperation _this, params IOperation[] subOperations)
        {
            yield return _this;
            foreach (var operation in subOperations)
                yield return operation;
        }

        /// <summary>
        /// プロパティ名からプロパティ設定オペレーションを作成する
        /// </summary>
        public static IOperation GenerateSetOperation<T, TProperty>(this T _this, string propertyName, TProperty newValue, TimeSpan timeSpan)
        {
            var oldValue = (TProperty)FastReflection.GetProperty(_this, propertyName);

            return GenerateAutoMergeOperation(_this, propertyName, newValue, oldValue, $"{_this.GetHashCode()}.{propertyName}", timeSpan);
        }

        public static IOperation GenerateSetOperation<T, TProperty>(this T _this, string propertyName, TProperty newValue)
        {
            return GenerateSetOperation(_this, propertyName, newValue, Operation.DefaultMergeSpan);
        }

        public static IOperation GenerateSetOperation<T, TProperty>(this T _this, Expression<Func<T, TProperty>> selector, TProperty newValue)
        {
            var propertyName = selector.GetMemberName();
            
            return GenerateSetOperation(_this, propertyName, newValue , Operation.DefaultMergeSpan);
        }

        /// <summary>
        /// マージ可能なオペレーションを作成する
        /// </summary>
        public static IOperation GenerateAutoMergeOperation<T, TProperty,TMergeKey>(this T _this,string propertyName, TProperty newValue ,TProperty oldValue, TMergeKey mergeKey,TimeSpan timeSpan)
        {
            return new MergeableOperation<TProperty>(
                x => { FastReflection.SetProperty(_this, propertyName, x); },
                newValue,
                oldValue, new ThrottleMergeJudge<TMergeKey>(mergeKey, timeSpan));
        }

        /// <summary>
        /// オペレーションに実行後イベントを追加する
        /// </summary>
        public static IOperation AddPostEvent(this IOperation _this, Action action)
        {
            if (_this is IOperationWithEvent triggerOperation)
            {
                triggerOperation.OnExecuted += action;
                return _this;
            }

            return new DelegateOperation(
                () =>
                {
                    _this.RollForward();
                    action.Invoke();
                },
                () =>
                {
                    _this.Rollback();
                    action.Invoke();
                });
        }

        /// <summary>
        /// オペレーション実行前にイベントを追加する
        /// </summary>
        public static IOperation AddPreEvent(this IOperation _this, Action action)
        {
            if (_this is IOperationWithEvent triggerOperation)
            {
                triggerOperation.OnPreviewExecuted += action;
                return _this;
            }

            return new DelegateOperation(
                () =>
                {
                    action.Invoke();
                    _this.RollForward();
                },
                () =>
                {
                    action.Invoke();
                    _this.Rollback();
                });
        }

        /// <summary>
        /// オペレーションが空かどうか
        /// </summary>
        public static bool IsEmpty(this IOperation _this)
        {
            if (_this == Operation.Empty)
                return true;
            if (_this is CompositeOperation _compositeOperation)
                return _compositeOperation.Any();
            return false;
        }
    }
    
    /// <summary>
    /// リスト操作オペレーション拡張
    /// </summary>
    public static class ListOperationExtensions
    {
        /// <summary>
        /// 値の追加オペレーションを作成する
        /// </summary>
        public static IOperation ToAddOperation<T>(this IList<T> _this, T value)
            => new InsertOperation<T>(_this, value);

        /// <summary>
        /// 値の削除オペレーションを作成する
        /// </summary>
        public static IOperation ToRemoveOperation<T>(this IList<T> _this, T value)
            => new RemoveOperation<T>(_this, value);

        /// <summary>
        /// 値のインデックス指定削除オペレーションを作成する
        /// </summary>
        public static IOperation ToRemoveAtOperation(this IList _this, int index)
            => new RemoveAtOperation(_this, index);

        /// <summary>
        /// 値の複数追加オペレーションを作成する
        /// </summary>
        public static IOperation ToAddRangeOperation<T>(this IList<T> _this, params T[] values)
            => ToAddRangeOperation(_this, values as IEnumerable<T>);

        public static IOperation ToAddRangeOperation<T>(this IList<T> _this, IEnumerable<T> values)
        {
            return values
                .Select(x => new InsertOperation<T>(_this, x))
                .ToCompositeOperation();
        }

        /// <summary>
        /// 値の複数削除オペレーションを作成する
        /// </summary>
        public static IOperation ToRemoveRangeOperation<T>(this IList<T> _this, params T[] values)
            => ToRemoveRangeOperation(_this, values as IEnumerable<T>);

        public static IOperation ToRemoveRangeOperation<T>(this IList<T> _this, IEnumerable<T> values)
        {
            return values
                .Select(x => new RemoveOperation<T>(_this, x))
                .ToCompositeOperation();
        }

        public static IOperation ToClearOperation<T>(this IList<T> _this)
            => new ClearOperation<T>(_this);
    }

    /// <summary>
    /// 階層オペレーション拡張
    /// </summary>
    public static class CompositeOperationExtensions
    {
        /// <summary>
        /// 複数のオペレーションをグループ化して１つのオペレーションに変換する
        /// </summary>
        public static ICompositeOperation ToCompositeOperation(this IEnumerable<IOperation> operations)
        {
            return new CompositeOperation(operations.ToArray());
        }

        /// <summary>
        /// オペレーションを結合して１つのオペレーションに変換する
        /// </summary>
        public static ICompositeOperation Union(this IOperation _this, params IOperation[] operations)
        {
            return new CompositeOperation(_this.CombineOperations(operations).ToArray());
        }

        public static ICompositeOperation Union(this IOperation _this, IEnumerable<IOperation> operations)
        {
            return Union(_this, operations.ToArray());
        }
    }
}
