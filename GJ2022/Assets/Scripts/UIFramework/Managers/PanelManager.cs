using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 面板管理器，用栈来存储UI
/// </summary>
public class PanelManager
{
    private Stack<basePanel> stackPanel;
    private UIManager uIManager;
    private BasePanel panel;
}
