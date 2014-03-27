using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DirectoryWatch
{
    public enum FileState
    {
        IsNew,
        IsDeleted,
        IsModified
    }

    public class ValueDifference<T>
    {
        public FileState state;
        public FileState State
        {
            get
            {
                return state;
            }
            set
            {
                state = (FileState)value;
            }
        }
        public T valueInfo;
        public T ValueInfo
        {
            get
            {
                return valueInfo;
            }
            set
            {
                valueInfo = (T)value;
            }
        }


        public ValueDifference(bool _isNew, bool _isDeleted, T _valueInfo)
        {
            if (_isNew)
            {
                State = FileState.IsNew;
            }
            if (_isDeleted)
            {
                State = FileState.IsDeleted;
            }
            if (!_isNew && !_isDeleted)
            {
                State = FileState.IsModified;
            }
            valueInfo = _valueInfo;
        }
    }

    public static class DictionaryExtensions
    {
        public static List<ValueDifference<TValue>> CompareDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dico, Dictionary<TKey, TValue> dico2, IEqualityComparer<TValue> comparer)
        {
            List<ValueDifference<TValue>> diff = new List<ValueDifference<TValue>>();
            List<TKey> oldKeys = dico2.Keys.ToList();
            foreach (KeyValuePair<TKey, TValue> _keypair in dico)
            {
                if (dico2.ContainsKey(_keypair.Key))
                {
                    bool _equality = comparer.Equals(_keypair.Value, dico2[_keypair.Key]);
                    if (!_equality)
                    {
                        diff.Add(new ValueDifference<TValue>(false, false, _keypair.Value));
                    }
                    oldKeys.Remove(_keypair.Key);
                }
                else
                {
                    diff.Add(new ValueDifference<TValue>(true, false, _keypair.Value));
                }
            }
            foreach (TKey _remainingKey in oldKeys)
            {
                diff.Add(new ValueDifference<TValue>(false, true, dico2[_remainingKey]));
            }
            return diff;
        }
    }
}
