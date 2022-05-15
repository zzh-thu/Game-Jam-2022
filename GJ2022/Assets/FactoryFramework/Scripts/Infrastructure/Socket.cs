using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{
    [System.Serializable]
    public abstract class Socket : MonoBehaviour
    {
        public virtual void Connect(Object obj) { }
        public virtual bool IsOpen() { return false; }
    }
}