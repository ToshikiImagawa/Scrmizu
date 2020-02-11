// Scrmizu C# reference source
// Copyright (c) 2016-2020 COMCREATE. All rights reserved.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Scrmizu.Sample.DI
{
    public abstract class Container : MonoBehaviour
    {
        private readonly Dictionary<Type, Func<object>> _factoryDictionary = new Dictionary<Type, Func<object>>();
        private bool _isInitialize;

        protected abstract void Install();

        private void Init()
        {
            if (_isInitialize) return;
            _isInitialize = true;
            Install();
        }

        public void Inject(object obj)
        {
            Init();
            var type = obj.GetType();
            var fields = type.GetFields(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly);
            foreach (var fieldInfo in fields)
            {
                if (fieldInfo.GetCustomAttribute<InjectField>() == null) continue;
                if (!_factoryDictionary.ContainsKey(fieldInfo.FieldType)) continue;
                fieldInfo.SetValue(obj, _factoryDictionary[fieldInfo.FieldType]());
            }
        }

        protected void Bind<T, TInstance>() where TInstance : T, new()
        {
            Bind<T>(() => new TInstance());
        }

        protected void BindAsSingle<T, TInstance>() where TInstance : T, new()
        {
            BindAsSingle<T>(() => new TInstance());
        }

        protected void BindAsSingleLazy<T, TInstance>() where TInstance : T, new()
        {
            BindAsSingleLazy<T>(() => new TInstance());
        }

        protected void Bind<T>() where T : new()
        {
            Bind(() => new T());
        }

        protected void BindAsSingle<T>() where T : new()
        {
            BindAsSingle(() => new T());
        }

        protected void BindAsSingleLazy<T>() where T : new()
        {
            BindAsSingleLazy(() => new T());
        }

        protected void Bind<T>(Func<T> factory)
        {
            _factoryDictionary[typeof(T)] = () =>
            {
                var instance = factory();
                Inject(instance);
                return instance;
            };
        }

        protected void BindAsSingle<T>(Func<T> factory)
        {
            var instance = factory();
            Inject(instance);
            _factoryDictionary[typeof(T)] = () => instance;
        }

        protected void BindAsSingleLazy<T>(Func<T> factory)
        {
            var lazy = new Lazy<T>(() =>
            {
                var instance = factory();
                Inject(instance);
                return instance;
            });
            _factoryDictionary[typeof(T)] = () => lazy.Value;
        }
    }
}