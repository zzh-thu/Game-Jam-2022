using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Inventory: MonoBehaviour
{
    public class Task
    {
        // requirements
        public int[] RawAmounts = new int[numOfRaw];
        public float[] RawEfficiencies = new float[numOfRaw];

        public int RobotAmount;
        public float RobotEfficiency;

        public int[] ProductionAmount = new int[numOfProduction];
        public float[] ProductionEfficiencies = new float[numOfProduction];

        public int[] SpecialAmount = new int[numOfSpecial];
        
        // attributes
        public int RewardScore;
    }
    
    // public parameters, TODO
    public static int numOfRaw = 2;
    public static int numOfProduction = 2;
    public static int numOfSpecial = 2;
    
    public static int minSpecialByEmergency;
    public static int maxSpecialByEmergency;
    public static int minRawByRecycle;
    public static int maxRawByRecycle;
    
    // records of amounts and efficiencies
    public int[] rawAmounts = new int[numOfRaw];
    public float[] rawEfficiencies = new float[numOfRaw];

    public int robotAmount;
    public float robotEfficiency;

    public int[] productionAmount = new int[numOfProduction];
    public float[] productionEfficiencies = new float[numOfProduction];
    public int[] productionScore = new int[numOfProduction];

    public int[] specialAmount = new int[numOfSpecial];
    public int[] specialNeededForCreate = new int[numOfSpecial];
    
    // bufferedRobot
    public Robot bufferedRobot;
    public GameObject robotObject;

    // control score and tasks.
    public int finalTargetScore;
    public int score;
    public int taskDurationInSeconds;
    public static int taskNum = 2;
    public int accomplishedTaskNum;
    public Task[] Tasks = new Task[taskNum];
    
    // timer
    public float time;

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
                robotAmount += 1;
                _GenerateBufferedRobot();
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
    
    // Try to generate next bufferedRobot.
    private void _GenerateBufferedRobot()
    {
        if (robotAmount == 0 || !ReferenceEquals(bufferedRobot, null)) return;
        
        var robotObj = Instantiate(robotObject, new Vector3(0, -100, 0), Quaternion.identity);
        foreach (var i in robotObj.GetComponentsInChildren<MeshRenderer>())  // turn off MeshRenderer
            i.enabled = false;
        bufferedRobot = robotObj.GetComponent<Robot>();
        bufferedRobot.state = Robot.RobotState.Sleepy;  // set state of Robot to sleepy
    }
    
    // Sell bufferedRobot, generate a new one if still robotAmount > 0;
    public void SellBufferedRobot()
    {
        if (robotAmount == 0) return;
        score += bufferedRobot.price;
        --robotAmount;
        _GenerateBufferedRobot();
    }
    
    // Called right after the player places the bufferedRobot in a WorkArea or sells it.
    // It will return the bufferedRobot being placed for binding to WorkArea or for reading its value,
    // and generate a new bufferedRobot if robotAmount > 0.
    public Robot FetchBufferedRobot()
    {
        if (bufferedRobot == null) return null;
        
        Robot ret = bufferedRobot;
        if (robotAmount > 0) --robotAmount;
        _GenerateBufferedRobot();

        return ret;
    }

    public void CreateRobotFromSpecial()
    {
        for (int i = 0; i < numOfSpecial; ++i)
            if (specialAmount[i] < specialNeededForCreate[i]) return;
        for (int i = 0; i < numOfSpecial; ++i)
            specialAmount[i] -= specialNeededForCreate[i];
        ++robotAmount;
        _GenerateBufferedRobot();
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
        // TODO: check time for tasks
        // t += Time.deltaTime;
        // if (t >= 5)
        // {
        //     t = 0;
        //     Debug.LogFormat("Inventory:\n" +
        //                     "   RawAmounts: {0}, {1}\n" +
        //                     "   RawEfficiencies: {2}, {3}\n",
        //         rawAmounts[0], rawAmounts[1], rawEfficiencies[0], rawEfficiencies[1]);
        // }
        time += Time.deltaTime;
    }
}
