using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class Inventory
{
    public int[] RawAmounts = new int[2];
    public float[] RawEfficiencies = new float[2];

    public int RobotAmount;
    public float RobotEfficiency;

    public float[] ProductionEfficiencies = new float[2];

    private Robot _bufferedRobot;

    public int Score;

    // singleton
    private Inventory() {}
    private static class SingletonInstance
    {
        public static readonly Inventory Instance = new Inventory();  // TODO: readonly
    }

    public static Inventory GetInventory()
    {
        return SingletonInstance.Instance;
    }
    
    public Robot FetchBufferedRobot()
    {
        if (_bufferedRobot == null) return null;
        
        Robot ret = _bufferedRobot;
        if (RobotAmount > 0)
        {
            // TODO
            // _bufferedRobot = 
            --RobotAmount;
        }

        return ret;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
