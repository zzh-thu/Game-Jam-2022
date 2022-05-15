using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{
    [CreateAssetMenu(menuName = "Factory Framework/Data/Recipe")]
    public class Recipe : ScriptableObject
    {
        public int numInputs = 1;
        public int numOutputs = 1;
        public ItemStack[] inputs;
        public ItemStack[] outputs;
        public float tickCost = 30f;

        public Item[] InputItems
        {
            get { 
                Item[] items = new Item[inputs.Length];
                for (int i = 0; i < inputs.Length; i++)
                {
                    items[i] = inputs[i].item;
                }
                return items;
            }
        }
        public Item[] OutputItems
        {
            get
            {
                Item[] items = new Item[outputs.Length];
                for (int i = 0; i < outputs.Length; i++)
                {
                    items[i] = outputs[i].item;
                }
                return items;
            }
        }
    }
}