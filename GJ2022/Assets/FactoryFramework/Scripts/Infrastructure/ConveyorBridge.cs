using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FactoryFramework
{
    public class ConveyorBridge : Socket
    {
        [Header("Properties")]
        public Conveyor connectingConveyor;
        private Conveyor thisConveyor;
        [SerializeField] private GameObject indicator;

        private void Awake()
        {
            thisConveyor = GetComponentInParent<Conveyor>();
        }

        public override void Connect(Object obj)
        {
            if (!(obj is Conveyor))
            {
                Debug.LogWarning($"Cannot Set Source because object {obj} is not a Conveyor type");
                return;
            }
            connectingConveyor = obj as Conveyor;
        }
        public override bool IsOpen()
        {
            return connectingConveyor == null;
        }

        private void Update()
        {
            if (thisConveyor == null || connectingConveyor == null)
            {
                indicator.SetActive(true);
                transform.position = thisConveyor.end;
                transform.rotation = Quaternion.LookRotation(thisConveyor.endDir,Vector3.up);
                return;
            }
            else

                indicator.SetActive(false);


            if (connectingConveyor.CanTakeInput(thisConveyor.OutputType()) && thisConveyor.CanGiveOutput())
            {
                connectingConveyor.TakeInput(thisConveyor.GiveOutput());
            }
        }
    }
}