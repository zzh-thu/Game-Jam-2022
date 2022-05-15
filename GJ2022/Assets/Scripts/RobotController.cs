using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RobotController : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Robot initial parameter")]
    public float Velocity4RawMaterial;
    public float Velocity4ProductCreate;
    public float Velocity4RobotCreate;

    [Space(50)]
    public float Price4Sell;





    void Start()
    {

    }

    void Update()
    {
        GetComponentInChildren<NavMeshAgent>();
    }
}

