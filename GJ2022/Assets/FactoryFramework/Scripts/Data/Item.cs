using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{

    [CreateAssetMenu(menuName = "Factory Framework/Data/Item")]
    public class Item : ScriptableObject
    {
        public Sprite icon;
        public GameObject prefab;
        public ItemData itemData;
        public Color DebugColor;
    }

    [System.Serializable]
    public struct ItemData
    {
        public string description;
        public int maxStack;
    }

    [System.Serializable]
    public struct ItemStack
    {
        public Item item;
        public int amount;
        public bool IsFull { get { return item != null && amount >= item.itemData.maxStack; } }

    
    }
    [System.Serializable]
    public struct ItemOnBelt
    {
        public Item item;
        public float position;
        public Transform model;
        public float EndPos { get { return position - ConveyorLogisticsUtils.settings.BELT_SPACING; } }
    }


}