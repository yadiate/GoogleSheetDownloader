using System;

namespace RIPinc.GoogleSheet
{
    public struct IfElseFloat
    {
        private readonly float[] _values;
        public IfElseFloat(params float[] values)
        {
            _values = values;
        }
        
        public int GetIndex(float value)
        {
            for (var i = 0; i < _values.Length; i++)
            {
                if (value <= _values[i])
                    return i;
            }
            return _values.Length;
        }
        public int GetIndexReverse(float value)
        {
            for (var i = _values.Length - 1; i >= 0; i--)
                if(value >= _values[i]) return i;

            return _values.Length;
        }

        public int GetIndex(float min, float max)
        {
            return GetIndex(UnityEngine.Random.Range(min,max));
        }
        
    }
    
    public struct IfElse<T,T1> where T : IComparable<T>
    {
        public           bool IsEmpty => _values == null || _values.Length == 0;
        private readonly T[]  _values;
        private readonly T1[] _results;
        
        public IfElse(T[] values, T1[] results)
        {
            _values = values;
            _results = results;
        }
        
        public T1 GetResult(T value)
        {
            for (var i = 0; i < _values.Length; i++)
            {
                if (value.CompareTo(_values[i]) <= 0)
                    return _results[i];
            }
            return _results[_values.Length];
        }
    }
}