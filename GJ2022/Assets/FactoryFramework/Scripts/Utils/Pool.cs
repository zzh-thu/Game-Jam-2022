using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{
    public class Pool<T>
    {
        public int capacity;
        protected readonly Stack<T> _stack = new Stack<T>();

        protected Func<T> _createFunc;
        protected Action<T> _getFunc;
        protected Action<T> _releaseFunc;
        protected Action<T> _destroyFunc;

        public Pool(Func<T> createFunc, Action<T> destroyFunc, Action<T> getFunc, Action<T> releaseFunc, int capacity=10) { 
            this.capacity = capacity;
            _createFunc = createFunc; 
            _getFunc = getFunc;
            _releaseFunc = releaseFunc;
            _destroyFunc = destroyFunc;
            _stack = new Stack<T>(capacity);
            Warm();
        }

        private void Warm()
        {
            for(int i = 0; i < capacity; i++)
            {
                T item = _createFunc();
                _releaseFunc(item);
                _stack.Push(item);
            }
        }

        public T GetItem()
        {
            if (_stack.Count == 0) { Debug.Log(_stack.Count); return default; }
            T item = _stack.Pop();
            _getFunc(item);
            return item;
        }

        public void ReleaseItem(T item)
        {
            _releaseFunc(item);
            _stack.Push(item);
        }
    }
}