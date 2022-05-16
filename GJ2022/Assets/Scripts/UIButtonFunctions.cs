using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UIButtonFunctions : MonoBehaviour
{
    public GameObject putRobotIcon;
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

    public void StartPutRobot()
    {
        Debug.Log("123123123123123123123123123");
        var icon = Instantiate(putRobotIcon);
        
        Debug.Log("123123123123123123123123123");

        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("inner click");
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit[] hits;
                hits = Physics.RaycastAll(ray);

                foreach (var hit in hits)
                {
                    if (hit.collider.GetComponent<RobotBase>())
                    {
                        var robot = Inventory.GetInventory().FetchBufferedRobot();
                        robot.transform.position = hit.collider.transform.position;  // TODO
                        break;
                    }
                }
                break;
            }
        }
        Destroy(icon);
    }
}
