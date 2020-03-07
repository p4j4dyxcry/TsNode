namespace TsCore.Operation
{
    public interface IMergeJudge

    {
        /// <summary>
        /// マージ可能か判断します。
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        bool CanMerge(IMergeJudge operation);
        
        /// <summary>
        /// ジャッジを更新します。マージCanMergeがfalseを返す場合も呼び出してください。
        /// </summary>
        /// <param name="prevMergeJudge">前回のマージ</param>
        /// <returns></returns>
        IMergeJudge Update(IMergeJudge prevMergeJudge);

        /// <summary>
        /// マージのためのKeyを取得
        /// </summary>
        object GetMergeKey();
    }
}
