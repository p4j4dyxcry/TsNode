using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TsGui.Operation
{
    /// <summary>
    /// 追加オペレーション
    /// </summary>
    public class InsertOperation<T> : IOperation
    {
        public string Messaage { get; set; }

        private readonly Func<IList<T>> _generator;
        private readonly IList<T> _list;
        private readonly T _property;
        private readonly int _insertIndex;

        private IList<T> get_list()
            => _list ?? _generator?.Invoke();


        public InsertOperation(Func<IList<T>> listGenerator, T insertValue , int insertIndex = -1)
        {
            Debug.Assert(listGenerator != null);
            _generator = listGenerator;
            _property = insertValue;
            _insertIndex = insertIndex;
        }

        public InsertOperation(IList<T> list, T insertValue, int insertIndex = -1)
        {
            Debug.Assert(list != null);
            _list = list;
            _property = insertValue;
            _insertIndex = insertIndex;
        }

        public void RollForward()
        {
            if(_insertIndex < 0)
                get_list().Add(_property);
            else
                get_list().Insert(_insertIndex,_property);
        }

        public void Rollback()
        {
            var list = get_list();
            list.RemoveAt( _insertIndex < 0 ? list.Count - 1 : _insertIndex );
        }
    }

    /// <summary>
    /// 削除オペレーション
    /// RollBack時に削除位置も復元する
    /// </summary>
    public class RemoveOperation<T> : IOperation
    {
        public string Messaage { get; set; }

        private readonly Func<IList<T>> _generator;
        private readonly IList<T> _list;
        private readonly T _property;
        private int _insertIndex = -1;

        private IList<T> get_list()
            => _list ?? _generator?.Invoke();
        
        public RemoveOperation(Func<IList<T>> listGenerator, T removeValue)
        {
            Debug.Assert(listGenerator != null);
            _generator = listGenerator;
            _property = removeValue;
        }

        public RemoveOperation(IList<T> list, T removeValue)
        {
            Debug.Assert(list != null);
            _list = list;
            _property = removeValue;
        }

        public void RollForward()
        {
            _insertIndex = get_list().IndexOf(_property);

            if (_insertIndex < 0)
                return;

            get_list().RemoveAt(_insertIndex);
        }

        public void Rollback()
        {
            if (_insertIndex < 0)
                return;

            get_list().Insert(_insertIndex, _property);
        }
    }

    /// <summary>
    /// インデックス指定削除オペレーション
    /// RollBack時に削除位置も復元する
    /// </summary>
    public class RemoveAtOperation : IOperation
    {
        public string Messaage { get; set; }

        private readonly Func<IList> _generator;
        private readonly IList _list;
        private object _data;
        private readonly int _index ;

        private IList get_list()
            => _list ?? _generator?.Invoke();

        public RemoveAtOperation(Func<IList> listGenerator, int index)
        {
            Debug.Assert(listGenerator != null);
            Debug.Assert(_index >= 0);
            _generator = listGenerator;
            _index = index;
        }

        public RemoveAtOperation(IList list, int index)
        {
            Debug.Assert(list != null);
            Debug.Assert(_index >= 0);
            _list = list;
            _index = index;
        }

        public void RollForward()
        {
            var list = get_list();
            _data = list[_index];
            list.RemoveAt(_index);
        }

        public void Rollback()
        {
            get_list().Insert(_index, _data);
        }
    }

    /// <summary>
    /// クリアオペレーション
    /// </summary>
    public class ClearOperation<T> : IOperation
    {
        public string Messaage { get; set; }

        private readonly Func<IList<T>> _generator;
        private readonly IList<T> _list;
        private T[] _prevData;

        private IList<T> get_list()
            => _list ?? _generator?.Invoke();


        public ClearOperation(Func<IList<T>> listGenerator)
        {
            Debug.Assert(listGenerator != null);
            _generator = listGenerator;
        }

        public ClearOperation(IList<T> list)
        {
            Debug.Assert(list != null);
            _list = list;
        }

        public void RollForward()
        {
            _prevData = get_list().ToArray();
            get_list().Clear();
        }

        public void Rollback()
        {
            foreach (var data in _prevData)
                get_list().Add(data);
        }
    }
}
