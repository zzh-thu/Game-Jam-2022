using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FactoryFramework
{
    [CreateAssetMenu(menuName = "Demo/Void Event Channel")]
    public class VoidEventChannel_SO : ScriptableObject
    {
        // Use this class to create "EventChannels" to raise or listen to events

        public UnityAction OnEvent;

        public virtual void Raise()
        {
            if (ConveyorLogisticsUtils.settings.SHOW_DEBUG_LOGS)
                Debug.Log($"Raising Void Event {name}");
            OnEvent?.Invoke();

        }
    }
}