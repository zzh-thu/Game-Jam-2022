using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using FactoryFramework;

public class MouseHoverBuildingTooltip : MonoBehaviour
{
    // requires some type of collider on this obj.
    // will not use RequiredComponent because one
    // gets added programatically in some cases in this demo
    private Text tooltip;

    private void Awake()
    {
        // haha don't do this 
        tooltip = GameObject.Find("Tooltip").GetComponent<Text>();
    }

    protected virtual string DisplayMessage()
    {
        if (TryGetComponent<Producer>(out Producer producer))
        {
            if (producer.resource.itemStack.item == null) return "";
            return $"{gameObject.name} producing {producer.resource.itemStack.amount} {producer.resource.itemStack.item.name}";
        }
        if (TryGetComponent<Processor>(out Processor _))
        {
            return $"{gameObject.name} processing items";
        }
        if (TryGetComponent<Splitter>(out Splitter _))
        {
            return $"{gameObject.name} splits conveyor belts";
        }
        if (TryGetComponent<Merger>(out Merger _))
        {
            return $"{gameObject.name} merges conveyor belts";
        }
        if (TryGetComponent<Storage>(out Storage _))
        {
            return $"{gameObject.name} can store items";
        }
        return "Invalid";
    }

    private void OnMouseOver()
    {
        tooltip.text = DisplayMessage();
    }
    private void OnMouseExit()
    {
        if (tooltip.text == DisplayMessage())
        {
            tooltip.text = "";
        }
    }
}
