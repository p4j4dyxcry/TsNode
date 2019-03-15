
namespace TsGui.Operation
{
    /// <summary>
    /// ロールバック可能な操作のインタフェース
    /// </summary>
    public interface IOperation
    {
        /// <summary>
        /// 表示名
        /// </summary>
        string Name { get; set; }

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
