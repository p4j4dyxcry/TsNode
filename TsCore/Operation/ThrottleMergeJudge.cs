using System;

namespace TsCore.Operation
{
    /// <summary>
    /// 識別子とタイムスタンプからマージ可能か判断する
    /// </summary>
    public class ThrottleMergeJudge<T> : IMergeJudge
    {
        public T Key { get; }

        /// <summary>
        /// マージ間隔
        /// </summary>
        public TimeSpan ConvergeTimeSpan { get; set; } 

        /// <summary>
        /// 操作の実行時間
        /// </summary>
        private DateTime TimeStamp { get; set; } = DateTime.Now;

        public bool CanMerge(IMergeJudge mergeJudge)
        {
            if (mergeJudge is ThrottleMergeJudge<T> timeStampMergeInfo)
            {
                return Equals(Key, timeStampMergeInfo.Key) &&
                       TimeStamp - timeStampMergeInfo.TimeStamp < ConvergeTimeSpan;
            }
            return false;
        }

        public ThrottleMergeJudge(T key , TimeSpan convergeTimeSpan)
        {
            Key = key;
            ConvergeTimeSpan = convergeTimeSpan;
        }

        public ThrottleMergeJudge(T key)
        {
            Key = key;
            ConvergeTimeSpan = Operation.DefaultMergeSpan;
        }

        public IMergeJudge Update(IMergeJudge prevMergeJudge)
        {
            if (prevMergeJudge is ThrottleMergeJudge<T> throttleMergeJudge)
            {
                throttleMergeJudge.TimeStamp = DateTime.Now;
                return throttleMergeJudge;
            }

            return this;
        }

        public object GetMergeKey()
        {
            return Key;
        }
    }
}
