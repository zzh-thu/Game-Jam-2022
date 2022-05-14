using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有UI面板的父类，包括UI面板的状态信息
/// </summary>
public class BasePanel
{
    /// <summary>
    /// UI信息
    /// </summary>
    /// <value></value>
    public UIType UIType { get; private set; }

    public BasePanel(UIType uIType)
    {
        UIType = uIType;
    }

    /// <summary>
    /// UI进入时执行的操作，只会执行一次
    /// </summary>
    public virtual void OnEnter() { }

    /// <summary>
    /// UI暂停时执行的操作
    /// </summary>
    public virtual void OnPause() { }
    /// <summary>
    /// UI继续时执行的操作
    /// </summary>
    public virtual void OnResume() { }
    /// <summary>
    /// UI退出时执行的操作
    /// </summary>
    public virtual void OnExit() { }

}
