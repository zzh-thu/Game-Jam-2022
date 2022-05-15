using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{
    public abstract class LogisticComponent : MonoBehaviour
    {
        protected GlobalLogisticsSettings settings { get { return ConveyorLogisticsUtils.settings; } }

        public virtual void ProcessLoop() { }

    }
}