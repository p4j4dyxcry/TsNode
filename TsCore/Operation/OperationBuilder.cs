using System;
using System.Collections.Generic;

namespace TsGui.Operation
{
    public partial class OperationBuilder
    {
        /// <summary>
        /// 実行手続きとロールバック手続きからオペレーションを作成
        /// </summary>
        public IOperationBuilder MakeFromAction(Action execute, Action rollback)
        {
            return new Builder(new DelegateOperation(execute, rollback));
        }

        /// <summary>
        /// 実行手続きとロールバック手続きからオペレーションを作成
        /// </summary>
        public IOperationBuilder MakeFromAction<T>(Action<T> execute, T newValue , T oldValue)
        {
            return new Builder(new DelegateOperation<T>(execute,newValue,oldValue));
        }

        /// <summary>
        /// 値が頻繁に変わるときに
        /// </summary>
        /// <param name="execute"></param>
        /// <param name="rollback"></param>
        /// <param name="key"></param>
        /// <param name="convergeTimeSpan"></param>
        /// <returns></returns>
        public IMergeableOperationBuilder MakeThrottle(Action execute, Action rollback, object key, TimeSpan convergeTimeSpan)
        {
            var judge = new ThrottleMergeJudge<int>(key.GetHashCode(), convergeTimeSpan);

            return new MergeableBuilder(new MergeableOperation(execute, rollback, judge));
        }

        public IMergeableOperationBuilder MakeThrottle<T>(Action<T> execute, T newValue ,T oldValue , object key, TimeSpan convergeTimeSpan)
        {
            var judge = new ThrottleMergeJudge<int>(key.GetHashCode(), convergeTimeSpan);

            return new MergeableBuilder<T>(new MergeableOperation<T>(execute, newValue,oldValue, judge));
        }

        public IMergeableOperationBuilder MakeMergeable(Action execute, Action rollback, object key)
        {
            return MakeThrottle(execute, rollback, key, Operation.DefaultMergeSpan);
        }

        /// <summary>
        /// 実行とロールバック手続きが同一処理の場合はこの関数の利用を推奨
        /// </summary>
        public IBuilderFromValues<T> MakeFromAction<T>(Action<T> action)
        {
            return new OperationBuilderFromValues<T>(action);
        }

        /// <summary>
        /// オペレーションがプロパティに値を設定する場合はこの関数の利用を推奨
        /// </summary>
        public IBuilderFromNewValue<TProperty> MakeFromProperty<TProperty>(object sender, string propertyName)
        {
            return new BuilderFromNewOperationValue<TProperty>(sender, propertyName);
        }

        public ICollectionOperationCustomizer<T> MakeCollectionOperationCustomizer<T>()
        {
            return new CollectionOperationCustomizer<T>();
        }

        public ICollectionOperationBuilder<T> MakeFromCollection<T>(IList<T> list)
        {
            return new CollectionOperationBuilder<T>(list);
        }
    }
}
