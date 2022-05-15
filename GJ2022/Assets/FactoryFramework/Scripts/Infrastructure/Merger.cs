using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{
    public class Merger : Building, IOutput, IInput
    {
        [SerializeField] private ConveyorSocket[] inputs;
        [SerializeField] private ConveyorSocket output;
        [SerializeField] private int inputIndex = 0; // modulo this number by inputs.Length

        public bool CanGiveOutput(Item filter = null)
        {
            if (output.conveyor == null || !output.conveyor.CanTakeInput(null)) return false;
            if (filter != null) Debug.LogWarning("Merger Does not Implement Item Filter Output");
            foreach (ConveyorSocket c in inputs)
                if (c.conveyor != null && c.conveyor.CanGiveOutput())
                    return true;
            return false;
        }
        // output type doesn't really matter
        public Item OutputType() { return inputs[inputIndex].conveyor.OutputType(); }
        public Item GiveOutput(Item filter = null)
        {
            if (filter != null) Debug.LogWarning("Merger Does not Implement Item Filter Output");
            Item result = null;
            if (inputs[inputIndex].conveyor==null || !inputs[inputIndex].conveyor.CanGiveOutput())
                GoToNextAvilable();
            result = inputs[inputIndex].conveyor.GiveOutput();
            GoToNextAvilable();
            return result;
        }

        void GoToNextAvilable()
        {
            for (int a = 0; a < inputs.Length; a++)
            {
                // loop through until we find the inputs ready for output
                inputIndex = (inputIndex + 1) % inputs.Length;
                if (inputs[inputIndex].conveyor != null && inputs[inputIndex].conveyor.CanGiveOutput())
                    break;
            }
        }

        

        public override void ProcessLoop()
        {
            base.ProcessLoop();

            if (!CanTakeInput(null) || !CanGiveOutput(null)) return;

            GoToNextAvilable();
            var i = inputs[inputIndex];
            Conveyor inputConveyor = i.conveyor;
            Conveyor outputConveyor = output.conveyor;

            outputConveyor.TakeInput(inputConveyor.GiveOutput());
        }

        private void OnDrawGizmos()
        {
            // doesnt matter item type
            if (CanGiveOutput())
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

        public void TakeInput(Item item)
        {
            output.conveyor.TakeInput(item);
        }

        public bool CanTakeInput(Item item)
        {
            return output.conveyor != null && output.conveyor.CanTakeInput(item);
        }
    }
}