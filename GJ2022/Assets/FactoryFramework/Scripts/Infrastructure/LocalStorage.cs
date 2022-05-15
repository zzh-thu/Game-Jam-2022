using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{
    [System.Serializable]
    public class LocalStorage : IInput, IOutput
    {
        public ItemStack itemStack;
        public bool overrideMaxStack = false;
        [Min(1)]
        public int overrideMaxStackNum = 1;

        public bool CanGiveOutput(Item filter = null) { return itemStack.item != null && itemStack.amount > 0; }
        public bool CanTakeInput(Item item)
        {
            // conditions to overwrite stack with new one
            if (itemStack.item == null) return true;
            if (itemStack.amount == 0) return true;

            // conditions where item cannot be added to stack
            if (itemStack.item != item && item != null) return false; // type mismatch
            int maxStack = (overrideMaxStack) ? overrideMaxStackNum : itemStack.item.itemData.maxStack;
            if (itemStack.amount >= maxStack) return false; // no room in stack

            return true;
        }
        public void TakeInput(Item item)
        {
            Debug.Assert(CanTakeInput(item), "No room for input");
            if (itemStack.item == null || itemStack.amount == 0)
            {
                // new stack!
                itemStack.item = item;
                itemStack.amount = 1;
            }
            else
            {
                itemStack.amount += 1;
            }
        }
        public Item OutputType() { return itemStack.item; }
        public Item GiveOutput(Item filter = null)
        {
            Debug.Assert(CanGiveOutput(), "No Output Available");

            itemStack.amount -= 1;
            return itemStack.item;
        }
    }
}