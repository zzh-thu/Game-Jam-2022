using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{
    public interface IOutput
    {
        Item OutputType();
        Item GiveOutput(Item filter = null);
        bool CanGiveOutput(Item filter = null);
    }
}