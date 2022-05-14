using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
    public int x, y;
    public float[] rawEfficiencies = new float[2];
    public float robotEfficiency;
    public float[] productionEfficiencies = new float[2];
    public int patient;
    public int price;
        
    public Robot()
    {
        // TODO: fill in number
        for (int i = 0; i < rawEfficiencies.Length; ++i)
            rawEfficiencies[i] = Random.Range(0f, 1f);
        robotEfficiency = Random.Range(0f, 1f);
        for (int i = 0; i < rawEfficiencies.Length; ++i)
            productionEfficiencies[i] = Random.Range(0f, 1f);
        patient = Random.Range(50, 100);
        price = 0; // TODO: calculate the price
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // check the patient of robot
    }
}
