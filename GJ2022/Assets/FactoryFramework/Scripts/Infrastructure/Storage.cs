using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{
    public class Storage : Building, IInput, IOutput
    {
        public int capacity;
        public ItemStack[] storage;

        private void Awake()
        {
            storage = new ItemStack[capacity];
        }

        public bool CanTakeInput(Item item)
        {
            if (item == null) return false;
            foreach (ItemStack stack in storage)
            {
                if (stack.item == item && stack.amount < item.itemData.maxStack) return true;
                if (stack.item == null) return true;
            }
            return false;
        }
        public void TakeInput(Item item)
        {
            for (int s = 0; s < storage.Length; s++)
            {
                ItemStack stack = storage[s];
                if (stack.item == item && stack.amount < item.itemData.maxStack)
                {
                    stack.amount += 1;
                    storage[s] = stack;
                    return;
                }
                if (stack.item == null)
                {
                    stack.item = item;
                    stack.amount = 1;
                    storage[s] = stack;
                    return;
                }
            }
        }

        public bool CanGiveOutput(Item filter = null)
        {
            foreach (ItemStack stack in storage)
            {
                if ((filter != null && stack.item == filter || stack.item != null) && stack.amount > 0) return true;
            }
            return false;
        }
        public Item OutputType()
        {
            foreach (ItemStack stack in storage)
            {
                if (stack.item == null && stack.amount > 0) return stack.item;
            }
            return null;
        }
        public Item GiveOutput(Item filter = null)
        {
            for (int s = 0; s < storage.Length; s++)
            {
                ItemStack stack = storage[s];
                if ((filter != null && stack.item == filter || stack.item != null) && stack.amount > 0)
                {
                    stack.amount -= 1;
                    Item item = stack.item;
                    if (stack.amount == 0)
                        stack.item = null;
                    storage[s] = stack;
                    return item;
                }
            }
            return null;
        }
    }
}