using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Robot : MonoBehaviour
{
    public int x, y;
    public enum RobotState
    {
        Active,
        Sleepy,
        MovingToRecycle,
        MovingFromRecycle,
        Recycled,
        Recycling
    }
    
    // efficiency
    public float[] rawEfficiencies;
    public float robotEfficiency;
    public float[] productionEfficiencies;
    private float _efficiency;
    
    public float recycledProgress;
    public float recycledProgressNeeded;
    public Robot recycledAgent;
    
    public int price;

    public RobotState state;
    public float patience;
    public int debuffNum;
    private WorkArea _workArea;
    
    public void Bind(WorkArea workArea)
    {
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
    
    public float GetEfficiency()
    {
        if (patience <= 0) return 0;
        return _efficiency;
    }
    
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
                recycledAgent = _workArea.FindRecycledAgent(x, y);
                break;
            case RobotState.Sleepy:
                break;
            case RobotState.MovingToRecycle:
                // TODO
                break;
            case RobotState.MovingFromRecycle:
                // TODO
                break;
            case RobotState.Recycled:
                if (recycledAgent.state == RobotState.Recycling)
                {
                    recycledProgress += recycledAgent.robotEfficiency;
                    if (recycledProgress >= recycledProgressNeeded)
                        _workArea.ReleaseRecycledAgent(recycledAgent);
                }
                break;
            case RobotState.Recycling:
                break;
        }
    }
}
