using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Robot : MonoBehaviour
{
    public enum RobotState
    {
        Active,
        Sleepy,
        MovingToRecycle,
        MovingFromRecycle,
        Recycled,
        Recycling
    }
    
    // efficiencies
    public float[] rawEfficiencies;
    public float robotEfficiency;
    public float[] productionEfficiencies;
    private float _efficiency;
    
    // other attributes
    public int x, y; // coordination in WorkArea
    public int price;
    private WorkArea _workArea;
    
    // related to being recycled
    public float recycledProgress;
    public float recycledProgressNeeded;
    public Robot recycledAgent;
    public bool isEmergency;
    
    // state
    public RobotState state;
    public float patience;
    public int debuffNum;
    
    // Bind this Robot to the WorkArea it belongs, which means:
    // 1. Saving a reference of WorkArea
    // 2. Deciding the actual efficiency
    public void Bind(WorkArea workArea)
    {
        _workArea = workArea;
        int workTypeNumber = workArea.workTypeNumber;
        switch (workArea.workType)
        {
            case WorkArea.WorkType.Raw:
                _efficiency = rawEfficiencies[workTypeNumber];
                break;
            case WorkArea.WorkType.Robot:
                _efficiency = robotEfficiency;
                break;
            case WorkArea.WorkType.Production:
                _efficiency = productionEfficiencies[workTypeNumber];
                break;
        }
    }

    // Denotes whether it exerts debuff on adjacent robots(if exist).
    public bool SpreadsDebuff()
    {
        return patience <= 0;
    }
    
    // Return efficiency, which denotes the efficiency of:
    // 1. Recycling, when it's doing recycling, or
    // 2. Working, when it has other states including even sleeping, for
    //    the convenience of calculating the efficiency.
    public float GetEfficiency()
    {
        switch (state)
        {
            case RobotState.Recycling:
                return robotEfficiency;
            default:
                return _efficiency;
        }
    }

    // Called when player decides to recycle this Robot.
    // Return whether an active Robot can be found as recycleAgent.
    public bool Recycle(bool isEmergentDissembling)
    {
        isEmergency = isEmergentDissembling;
        recycledAgent = _workArea.FindRecycledAgent(this);
        if (!ReferenceEquals(recycledAgent, null))
        {
            state = RobotState.Recycled;
            return true;
        }
        return false;
    }
    
    // ======== common ========
    
    void Start()
    {
        // initiate
        rawEfficiencies = new float[Inventory.numOfRaw];
        productionEfficiencies = new float[Inventory.numOfProduction];
        
        // TODO: fill in number
        for (int i = 0; i < Inventory.numOfRaw; ++i)
            rawEfficiencies[i] = 0.2f;
        robotEfficiency = 0.2f;
        for (int i = 0; i < Inventory.numOfProduction; ++i)
            productionEfficiencies[i] = 0.2f;
        patience = 100f;
        price = 0; // TODO: calculate the price
    }

    void Update()
    {
        switch (state)
        {
            case RobotState.Active:
                patience -= Time.deltaTime * (1 + debuffNum);
                if (patience <= 0) state = RobotState.Sleepy;
                _workArea.RobotSleep(this);
                break;
            case RobotState.Sleepy:
                break;
            case RobotState.MovingToRecycle:
                // TODO: check navigation state
                break;
            case RobotState.MovingFromRecycle:
                // TODO: check navigation state
                break;
            case RobotState.Recycled:
                if (recycledAgent.state == RobotState.Recycling)
                {
                    recycledProgress += recycledAgent.GetEfficiency();
                    if (recycledProgress >= recycledProgressNeeded)
                        _workArea.ReleaseRecycledAgent(recycledAgent);
                }
                break;
            case RobotState.Recycling:
                break;
        }
    }
}
