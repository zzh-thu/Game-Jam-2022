using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UIButtonFunctions : MonoBehaviour
{
    public GameObject canvas1;
    public GameObject canvas2;
    public void SellRobot()
    {
        Inventory.GetInventory().SellBufferedRobot();
    }

    public void CreateRobotFromSpecial()
    {
        Inventory.GetInventory().CreateRobotFromSpecial();
    }

    public void Pause()
    {
        Inventory.GetInventory().Pause();
    }

    public void Continue()
    {
        Inventory.GetInventory().Continue();
    }

    public void GoToLevelSelect()
    {
        canvas1.SetActive(false);
        canvas2.SetActive(true);
    }
}
