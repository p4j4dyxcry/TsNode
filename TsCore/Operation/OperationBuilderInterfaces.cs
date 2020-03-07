using System;
using System.Collections.Generic;

namespace TsCore.Operation
{
    /// <summary>
    /// オペレーションを作成する基本的なビルダー
    /// </summary>
    public interface IOperationBuilder
    {
        IOperationBuilder Message(string message);

        /// <summary>
        /// オペレーションの実行・ロールバック後にイベントを追加
        /// </summary>
        IOperationBuilder PostEvent(Action action);

        /// <summary>
        /// オペレーションの実行・ロールバック前にイベントを追加
        /// </summary>
        IOperationBuilder PrevEvent(Action action);

        /// <summary>
        /// オペレーションを作成
        /// </summary>
        IOperation Build();
    }

    public interface IMergeableOperationBuilder : IOperationBuilder
    {
        IMergeableOperationBuilder SetActionName(string executeAction, string rollbackAction);
    }

    public interface IBuilderFromValues<in T> : IOperationBuilder
    {
        /// <summary>
        /// 前回の値と新しい値を使用してビルダーを作成
        /// </summary>
        IBuilderFromValues<T> Values(T newValue, T prevValue);

        IBuilderFromValues<T> Throttle(object key , TimeSpan timeSpan);
    }

    public interface IBuilderFromNewValue<in T> : IOperationBuilder
    {
        /// <summary>
        /// 新しい値を利用してビルダーを作成
        /// </summary>
        IBuilderFromNewValue<T> NewValue(T newValue);

        IBuilderFromNewValue<T> Throttle(TimeSpan timeSpan);
    }

    /// <summary>
    /// コレクションを操作するオペレーションを作成するインタフェース
    /// </summary>
    public interface ICollectionOperationBuilder<in T>
    {
        /// <summary>
        /// コレクションに要素を追加するオペレーションを作成
        /// </summary>
        IOperation BuildAddOperation(T addValue);

        /// <summary>
        /// コレクションからに要素を削除するオペレーションを作成
        /// </summary>
        IOperation BuildRemoveOperation(T removeValue);

        /// <summary>
        /// コレクションに要素を複数追加するオペレーションを作成
        /// </summary>
        IOperation BuildAddRangeOperation(IEnumerable<T> addValue);

        /// <summary>
        /// コレクションから要素を複数削除するオペレーションを作成
        /// </summary>
        IOperation BuildRemoveRangeOperation(IEnumerable<T> removeValue);

        /// <summary>
        /// コレクションを空にするオペレーションを作成
        /// </summary>
        IOperation BuildClearOperation();
    }

    /// <summary>
    /// コレクション操作を独自定義してオペレーションを作成するビルダー
    /// </summary>
    public interface ICollectionOperationCustomizer<T> : ICollectionOperationBuilder<T>
    {
        /// <summary>
        /// 追加手続きを登録
        /// </summary>
        ICollectionOperationCustomizer<T> RegisterAdd(Action<T> value);
        ICollectionOperationCustomizer<T> RegisterAdd(Action function);

        /// <summary>
        /// 削除手続きを登録
        /// </summary>
        ICollectionOperationCustomizer<T> RegisterRemove(Action<T> function);
        ICollectionOperationCustomizer<T> RegisterRemove(Action function);

        /// <summary>
        /// 空にするための手続きを登録
        /// </summary>
        ICollectionOperationCustomizer<T> RegisterClear(Action function);

        /// <summary>
        /// 空にした要素を復元する手続きを登録
        /// </summary>
        ICollectionOperationCustomizer<T> RegisterRollback(Action function);
    }
}