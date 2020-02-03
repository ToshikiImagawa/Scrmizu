// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace Scrmizu.Sample
{
    public class SortedInfiniteScrollItemRepository<TKey, TValue> : IInfiniteScrollItemRepository where TValue : class
    {
        private readonly Func<TValue, TKey> _selector;

        public SortedInfiniteScrollItemRepository(Func<TValue, TKey> selector)
        {
            _selector = selector;
        }

        private readonly SortedList<TKey, object> _dataCache = new SortedList<TKey, object>();
        public int Count => _dataCache.Count;

        public object this[int index] => _dataCache.ElementAt(index);

        public void Add(object item)
        {
            if (item is TValue data) _dataCache.Add(_selector(data), item);
        }

        public void AddRange(IEnumerable<object> collection)
        {
            foreach (var item in collection)
            {
                if (item is TValue data) _dataCache.Add(_selector(data), item);
            }
        }

        public void Insert(int index, object item)
        {
            throw new NotSupportedException();
        }

        public void InsertRange(int index, IEnumerable<object> collection)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            _dataCache.RemoveAt(index);
        }

        public void RemoveRange(int index, int count)
        {
            var startIndex = index + count - 1;
            Assert.IsTrue(index >= 0, "index");
            Assert.IsTrue(count >= 0, "count");
            Assert.IsTrue(index < _dataCache.Count, "index");
            Assert.IsTrue(startIndex < _dataCache.Count, "count");
            for (var i = startIndex; i > index; i--)
            {
                _dataCache.RemoveAt(i);
            }
        }

        public void Clear()
        {
            _dataCache.Clear();
        }

        public object[] ToArray()
        {
            return _dataCache.Values.ToArray();
        }
    }
}