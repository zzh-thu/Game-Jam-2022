using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{
    public class Splitter : Building, IInput, IOutput
    {
        [SerializeField] private ConveyorSocket input;
        [SerializeField] private ConveyorSocket[] outputs;

        [SerializeField] private int outputIndex = 0; // modulo this number by output.Length

        public bool CanTakeInput(Item item)
        {
            foreach (ConveyorSocket c in outputs)
                if (c.conveyor != null && c.conveyor.CanTakeInput(item))
                    return true;
            return false;
        }
        public void TakeInput(Item item)
        {
            if (outputs[outputIndex].conveyor == null || !outputs[outputIndex].conveyor.CanTakeInput(item))
            {
                GoToNextAvilable(item);
            }
            outputs[outputIndex].conveyor.TakeInput(item);
            GoToNextAvilable(item);
            return;
        }

        public override void ProcessLoop()
        {
            base.ProcessLoop();

            if (!CanGiveOutput()) return;

            GoToNextAvilable(null);
            var o = outputs[outputIndex];
            Conveyor outputConveyor = o.conveyor;

            outputConveyor.TakeInput(GiveOutput(null));

        }

        void GoToNextAvilable(Item item)
        {
            for (int a = 0; a < outputs.Length; a++)
            {
                // loop through until we find the outputs ready to take input
                outputIndex = (outputIndex + 1) % outputs.Length;
                if (outputs[outputIndex].conveyor != null && outputs[outputIndex].conveyor.CanTakeInput(item))
                    return;
            }

        }

        private void OnDrawGizmos()
        {
            // doesnt matter item type
            if (CanTakeInput(null))
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 1f);
        }

        public Item OutputType()
        {
            return input.conveyor.OutputType();
        }

        public Item GiveOutput(Item filter = null)
        {
            return input.conveyor.GiveOutput(filter);
        }

        public bool CanGiveOutput(Item filter = null)
        {
            // if no conveyors have room, return false
            if (!CanTakeInput(null)) {            
                return false; 
            }
            if (input.conveyor != null && input.conveyor.CanGiveOutput(null)) return true;

            return false;

        }
    }
}