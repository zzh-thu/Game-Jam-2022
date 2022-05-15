using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{
    public interface IInput
    {
        void TakeInput(Item item);
        bool CanTakeInput(Item item);
    }
}