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
    public static int numOfSpecial = 2;
    
    // records of amounts and efficiencies
    public int[] rawAmounts = new int[numOfRaw];
    public float[] rawEfficiencies = new float[numOfRaw];

    public int robotAmount;
    public float robotEfficiency;

    public int[] productionAmount = new int[numOfProduction];
    public float[] productionEfficiencies = new float[numOfProduction];
    public int[] productionScore = new int[numOfProduction];

    public int[] specialAmount = new int[numOfSpecial];
    
    public Robot bufferedRobot;

    // control score and tasks.
    // TODO: tasks
    public int finalTargetScore;
    public int score;

    // ======== APIs for WorkArea when amounts or efficiencies change ========
    
    // Call by WorkArea when it finishes a job.
    public void AddAmount(WorkArea.WorkType workType, int workTypeNumber, int amount)
    {
        switch (workType)
        {
            case WorkArea.WorkType.Raw:
                rawAmounts[workTypeNumber] += 1;
                break;
            case WorkArea.WorkType.Robot:
                if (robotAmount == 0)
                    bufferedRobot = _GenerateBufferedRobot();  // generate bufferedRobot if it's null
                robotAmount += 1;
                break;
            case WorkArea.WorkType.Production:
                productionAmount[workTypeNumber] += 1;
                // TODO: increase score
                break;
        }
        // TODO: check current task
    }

    // Called by WorkArea whose total efficiency just changed.
    public void SetEfficiency(WorkArea.WorkType workType, int workTypeNumber, float value)
    {
        switch (workType)
        {
            case WorkArea.WorkType.Raw:
                rawEfficiencies[workTypeNumber] = value;
                break;
            case WorkArea.WorkType.Robot:
                robotEfficiency = value;
                break;
            case WorkArea.WorkType.Production:
                productionEfficiencies[workTypeNumber] = value;
                break;
        }
        // TODO: check current task
    }
    
    // ======== operations of bufferedRobot =========
    
    // Generate next bufferedRobot, return null if robotAmount == 0
    private Robot _GenerateBufferedRobot()
    {
        // TODO
        return null;
    }
    
    // Sell bufferedRobot, generate a new one if still robotAmount > 0;
    public void SellBufferedRobot()
    {
        score += bufferedRobot.price;
        --robotAmount;
        bufferedRobot = _GenerateBufferedRobot();
    }
    
    // Called right after the player places the bufferedRobot in a WorkArea or sells it.
    // It will return the bufferedRobot being placed for binding to WorkArea or for reading its value,
    // and generate a new bufferedRobot if robotAmount > 0.
    public Robot FetchBufferedRobot()
    {
        if (bufferedRobot == null) return null;
        
        Robot ret = bufferedRobot;
        if (robotAmount > 0) --robotAmount;
        bufferedRobot = _GenerateBufferedRobot();

        return ret;
    }

    // ======== singleton ========
    
    private static Inventory _instance;
    public static Inventory GetInventory() {
        return _instance;
    }
    protected virtual void Awake()
    {
        _instance = this;
    }

    // ======== common ========
    
    void Start()
    {
        
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
