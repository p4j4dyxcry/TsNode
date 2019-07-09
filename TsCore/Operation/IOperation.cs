
namespace TsGui.Operation
{
    /// <summary>
    /// ロールバック可能な操作のインタフェース
    /// </summary>
    public interface IOperation
    {
        /// <summary>
        /// メッセージ
        /// </summary>
        string Messaage { get; set; }

        /// <summary>
        /// 実行 / 前進回帰
        /// </summary>
        void RollForward();

        /// <summary>
        /// ロールバック
        /// </summary>
        void Rollback();
    }
}
