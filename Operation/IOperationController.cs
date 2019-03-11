using System;
using System.Collections.Generic;

namespace TsGui.Operation
{
    /// <summary>
    /// オペレーションを実行するコントローラ
    /// </summary>
    public interface IOperationController
    {
        /// <summary>
        /// 一つ前の処理へ戻れるか
        /// </summary>
        bool CanUndo { get; }

        /// <summary>
        /// 戻った処理をやりなおせるか
        /// </summary>
        bool CanRedo { get; }

        /// <summary>
        /// 先頭のオペレーションをロールバックする
        /// </summary>
        void Undo();

        /// <summary>
        /// ロールバックされたオペレーションをロールフォワードする
        /// </summary>
        void Redo();

        /// <summary>
        /// スタックをクリアする
        /// </summary>
        void Flush();

        /// <summary>
        /// スタックからデータを取り出さずにデータを取得する
        /// </summary>
        IOperation Peek();

        /// <summary>
        /// スタックからデータを取り出す
        /// </summary>
        IOperation Pop();

        /// <summary>
        /// 実行しないでスタックにデータを積む
        /// </summary>
        IOperation Push(IOperation operation);

        /// <summary>
        /// 操作を実行し、スタックに積む
        /// </summary>
        IOperation Execute(IOperation operation);

        /// <summary>
        /// 実行された操作一覧を取得する
        /// </summary>
        IEnumerable<IOperation> Operations { get; }

        /// <summary>
        /// ロールフォワード対象を取得する
        /// </summary>
        IEnumerable<IOperation> RollForwardTargets { get; }

        /// <summary>
        /// スタックが更新されたときにイベントが発生する
        /// </summary>
        event Action<object, OperationStackChangedEventArgs> StackChanged;

        /// <summary>
        /// オペレーション中かどうか
        /// </summary>
        bool IsOperating { get; }
    }

    public enum OperationStackChangedEvent
    {
        Undo,
        Redo,
        Push,
        Pop,
        Clear,
    }

    public class OperationStackChangedEventArgs : EventArgs
    {
        public OperationStackChangedEvent EventType { get; set; }
    }
}
