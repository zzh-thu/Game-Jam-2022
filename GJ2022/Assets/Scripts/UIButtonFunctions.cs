using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonFunctions : MonoBehaviour
{
    public void SellRobot()
    {
        Inventory.GetInventory().SellBufferedRobot();
    }

    public void CreateRobotFromSpecial()
    {
        Inventory.GetInventory().CreateRobotFromSpecial();
    }
}
