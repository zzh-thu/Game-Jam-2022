using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkArea : MonoBehaviour
{
    public enum WorkType
    {
        Raw,
        Robot,
        Production
    }

    // robots
    public int maxY;
    private Robot[,] _robots;

    // state
    private float _efficiency;
    private bool _isWorking;
    private float _progress;
    public bool isTurnedOn = true;

    // attributes
    public float neededProgress;
    public int[] inputRaw; // not used in WorkType == Raw
    public WorkType workType;
    public int workTypeNumber;
    // public int[] outputNum = new int[Inventory.NumOfProduction];  // not used in WorkType == Robot, TODO
    public int price;  // only used in WorkType == Production

    private void _InformEfficiency()
    {
        var inventory = Inventory.GetInventory();
        switch (workType)
        {
            case WorkType.Raw:
                inventory.rawEfficiencies[workTypeNumber] = _efficiency;
                break;
            case WorkType.Robot:
                inventory.robotEfficiency = _efficiency;
                break;
            case WorkType.Production:
                inventory.productionEfficiencies[workTypeNumber] = _efficiency;
                break;
        }
    }
    
    public void RemoveRobot(Robot robot)
    {
        // TODO: buff of robots
        switch (workType)
        {
            case WorkType.Raw:
                _efficiency -= robot.rawEfficiencies[workTypeNumber];
                break;
            case WorkType.Robot:
                _efficiency -= robot.robotEfficiency;
                break;
            case WorkType.Production:
                _efficiency -= robot.productionEfficiencies[workTypeNumber];
                break;
        }
        _InformEfficiency();
    }

    // ====    ====    ====    ====    ====    ====    ====    ====
    // Functions below are used during robots' state transition, to:
    // 1. Recalculate the efficiency of this WorkArea, or
    // 2. Inform other robots in this WorkArea
    
    // Check if there is a robot in its left
    private bool _HasRobotInLeft(int x, int y)
    {
        return 0 < y && _robots[x, y - 1] != null;
    }

    private bool _HasRobotInRight(int x, int y)
    {
        return y < maxY - 1 && _robots[x, y + 1] != null;
    }

    // Put a robot to a given position in this WorkArea
    public void PutRobot(int x, int y)
    {
        var robot = Inventory.GetInventory().FetchBufferedRobot();
        robot.Bind(this);
        _robots[x, y] = robot;
        
        if (_HasRobotInLeft(x, y) && _robots[x, y - 1].SpreadDebuff()) ++robot.debuffNum;
        if (_HasRobotInRight(x, y) && _robots[x, y + 1].SpreadDebuff()) ++robot.debuffNum;
        _efficiency += robot.GetEfficiency();
        
        _InformEfficiency();
    }

    public void RobotSleep(int x, int y)
    {
        _efficiency -= _robots[x, y].GetEfficiency();
        
        if (_HasRobotInLeft(x, y)) ++_robots[x, y - 1].debuffNum;
        if (_HasRobotInRight(x, y)) ++_robots[x, y + 1].debuffNum;
        
        _InformEfficiency();
    }

    public Robot FindRecycledAgent(int x, int y)
    {
        // TODO
        // find by distance
        // set its state, recalculate efficiency
        // navigate it to (x, y)
        return null;
    }

    public void ReleaseRecycledAgent(Robot recycledRobot)
    {
        // TODO:
        // navigate agent to (x, y)
        // set its state, recalculate efficiency
        // release recycledRobot
    }

    public void TurnOnOrOff()
    {
        isTurnedOn = !isTurnedOn;
    }

    void Start()
    {
        // initiate
        _robots = new Robot[2, maxY];
        inputRaw = new int[Inventory.numOfRaw];

        foreach (var robot in GetComponentsInChildren<Robot>())  // calculate the initial efficiency
        {
            robot.Bind(this);
            if (robot.state == Robot.RobotState.Active)
                _efficiency += robot.GetEfficiency();
        }
        _InformEfficiency();
    }

    // Try to start next work in Update
    private void _TryProduce()
    {
        var inventory = Inventory.GetInventory();
        for (int i = 0; i < inputRaw.Length; ++i)  // check whether each raw materials is enough
            if (inventory.rawAmounts[i] < inputRaw[i]) return;

        for (int i = 0; i < inputRaw.Length; ++i)  // deduct materials from inventory
            inventory.rawAmounts[i] -= inputRaw[i];

        _isWorking = true;
    }
    
    private float t = 0f;
    void Update()
    {
        t += Time.deltaTime;
        if (t >= 4)
        {
            t = 0;
            Debug.LogFormat("WorkArea:" +
                            "   _efficiency: {0}\n" +
                            "   _isWorking: {1}\n" +
                            "   _progress: {2}\n",
                _efficiency, _isWorking, _progress);
        }
        
        if (!isTurnedOn) return;
        
        if (_isWorking)
        {
            _progress += Time.deltaTime * _efficiency;
            if (_progress < neededProgress) return;
            
            // work has been finished, inform the inventory
            var inventory = Inventory.GetInventory();
            switch (workType)
            {
                case WorkType.Raw:
                    inventory.rawAmounts[workTypeNumber] += 1;
                    break;
                case WorkType.Robot:
                    inventory.robotAmount += 1;
                    break;
                case WorkType.Production:
                    inventory.robotAmount += 1;
                    break;
            }
            _progress = 0;  // change the state to idle
            _isWorking = false;
        }
        
        if (!_isWorking) _TryProduce();  // try to start next work if is idling
    }
}
