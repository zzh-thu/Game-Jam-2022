using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DragTheRobot2Space : MonoBehaviour
{
    [SerializeField] bool IsClick = false;

    public GameObject CurrentRobot;
    public RectTransform UIposition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0) && !IsClick)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.GetComponent<RobotController>())
                {
                    hit.collider.transform.transform.position = hit.transform.position;
                    IsClick = true;
                    CurrentRobot = hit.collider.gameObject;
                }
            }
        }

        if (IsClick)
        {
            Debug.Log("get mouse position");
            Vector3 ScreenSpace = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 MousPos = Camera.main.ScreenToWorldPoint(new Vector3( Input.mousePosition.x, Input.mousePosition.y, ScreenSpace.z));
            Debug.Log("mouse position is "+ Input.mousePosition);
            CurrentRobot.transform.position = new Vector3 ( MousPos.x, MousPos.y, CurrentRobot.transform.position.z);
        
        }

        if (Input.GetMouseButtonDown(0) && IsClick)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits;
            hits = Physics.RaycastAll(ray);

            foreach (var hit in hits)
            {
                if (hit.collider.GetComponent<RobotBase>())
                {
                    transform.position = hit.collider.transform.position;

                    IsClick = false;
                    break;
                }
            }
      
        }
    }

  
}
