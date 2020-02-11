// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;

namespace Scrmizu.Sample.DI
{
    public class Lazy<T>
    {
        private readonly Func<T> _getter;
        private bool _initialized;
        private T _catch;

        public T Value
        {
            get
            {
                if (!_initialized)
                {
                    _catch = _getter();
                }

                _initialized = true;
                return _catch;
            }
        }

        public Lazy(Func<T> getter)
        {
            _getter = getter;
            _initialized = false;
        }
    }
}