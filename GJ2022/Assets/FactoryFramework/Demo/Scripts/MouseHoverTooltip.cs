using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseHoverTooltip : MonoBehaviour
{
    // requires some type of collider on this obj.
    // will not use RequiredComponent because one
    // gets added programatically in some cases in this demo
    [SerializeField] string message = "Message Goes Here.";
    private Text tooltip;

    private void Awake()
    {
        tooltip = GameObject.Find("Tooltip").GetComponent<Text>();
    }

    protected virtual string DisplayMessage()
    {
        return message;
    }

    private void OnMouseEnter()
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
