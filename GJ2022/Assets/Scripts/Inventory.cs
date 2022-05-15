using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor;
using UnityEngine;

public class Inventory: MonoBehaviour
{
    public static int numOfRaw = 2;
    public static int numOfProduction = 2;

    public int[] rawAmounts = new int[numOfRaw];
    public float[] rawEfficiencies = new float[numOfRaw];

    public int robotAmount;
    public float robotEfficiency;

    public int[] productionAmount = new int[numOfProduction];
    public float[] productionEfficiencies = new float[numOfProduction];

    // TODO: special materials
    
    private Robot _bufferedRobot;

    public int score;

    // singleton
    private static Inventory _instance;
    public static Inventory GetInventory() {
        return _instance;
    }

    
    public Robot FetchBufferedRobot()
    {
        if (_bufferedRobot == null) return null;
        
        Robot ret = _bufferedRobot;
        if (robotAmount > 0)
        {
            // TODO
            // _bufferedRobot = 
            --robotAmount;
        }

        return ret;
    }

    protected virtual void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        // rawAmounts = new int[numOfRaw];
        // rawEfficiencies = new float[numOfRaw];
        // productionEfficiencies = new float[numOfProduction];
    }

    private float t = 0;
    void Update()
    {
        t += Time.deltaTime;
        if (t >= 5)
        {
            t = 0;
            Debug.LogFormat("Inventory:\n" +
                            "   RawAmounts: {0}, {1}\n" +
                            "   RawEfficiencies: {2}, {3}\n",
                rawAmounts[0], rawAmounts[1], rawEfficiencies[0], rawEfficiencies[1]);
        }
    }
}
