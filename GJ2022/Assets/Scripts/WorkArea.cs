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

    // Robots
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

    // Change recorded efficiency in Inventory.
    private void _InformEfficiency()
    {
        Inventory.GetInventory().SetEfficiency(workType, workTypeNumber, _efficiency);
    }

    // Turn on or turn off this WorkArea.
    public void TurnOnOrOff()
    {
        isTurnedOn = !isTurnedOn;
    }

    // ======== APIs for Robots' state transitions ========
    // Functions below are used during Robots' state transition, to:
    // 1. Recalculate the efficiency of this WorkArea, or
    // 2. Inform other Robots in this WorkArea.
    
    // Check if there is a Robot in this position.
    private bool _HasRobot(int x, int y)
    {
        return 0 < y && y < maxY - 1 && ReferenceEquals(_robots[x, y - 1], null);
    }

    // Put a Robot to a given position in this WorkArea.
    public void PutRobot(int x, int y)
    {
        var robot = Inventory.GetInventory().FetchBufferedRobot();
        robot.Bind(this);
        _robots[x, y] = robot;
        
        if (_HasRobot(x, y - 1) && _robots[x, y - 1].SpreadsDebuff()) ++robot.debuffNum;
        if (_HasRobot(x, y + 1) && _robots[x, y + 1].SpreadsDebuff()) ++robot.debuffNum;
        _efficiency += robot.GetEfficiency();
        
        _InformEfficiency();
    }

    // Called by a Robot when its patience drops to 0.
    public void RobotSleep(Robot robot)
    {
        int x = robot.x, y = robot.y;
        _efficiency -= robot.GetEfficiency();

        if (_HasRobot(x, y - 1)) ++_robots[x, y - 1].debuffNum;
        if (_HasRobot(x, y + 1)) ++_robots[x, y + 1].debuffNum;
        
        _InformEfficiency();
    }

    // Called by a Robot when the player decides to recycle it, by clicking it or somehow.
    public Robot FindRecycledAgent(Robot recycledRobot)
    {
        int x = recycledRobot.x, y = recycledRobot.y;
        if (!recycledRobot.SpreadsDebuff()) _efficiency -= recycledRobot.GetEfficiency();  // recycle an active Robot

        // find the closet active 
        Robot agent = null;
        for (int i = 1; i < maxY; ++i)
        {
            if (_HasRobot(x, y - i) && _robots[x, y - i].state == Robot.RobotState.Active)
            {
                agent = _robots[x, y - i];
                break;
            }

            if (_HasRobot(x, y + i) && _robots[x, y + i].state == Robot.RobotState.Active)
            {
                agent = _robots[x, y + i];
                break;
            }
        }
        if (ReferenceEquals(agent, null)) return null;
        
        _efficiency -= agent.GetEfficiency();
        agent.state = Robot.RobotState.MovingToRecycle;
        // TODO: navigate it to (x, y)
        return agent;
    }

    // Called by a recycled Robot when the recycle is finished.
    public void ReleaseRecycledAgent(Robot recycledRobot)
    {
        var agent = recycledRobot.recycledAgent;
        // TODO: navigate agent to (x, y)
        agent.state = Robot.RobotState.MovingFromRecycle;
        
        int x = recycledRobot.x, y = recycledRobot.y;
        _robots[x, y] = null;
        
        // TODO: add raw or special materials to inventory
    }
    
    // ======== common ========

    void Start()
    {
        // initiate
        _robots = new Robot[2, maxY];
        inputRaw = new int[Inventory.numOfRaw];

        foreach (var robot in GetComponentsInChildren<Robot>())  // calculate the initial efficiency
        {
            robot.Bind(this);
            _efficiency += robot.GetEfficiency();
        }
        
        _InformEfficiency();
    }

    // Try to start next work, called in Update.
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
                    inventory.productionAmount[workTypeNumber] += 1;
                    break;
            }
            _progress = 0;  // change the state to idle
            _isWorking = false;
        }
        
        if (!_isWorking) _TryProduce();  // try to start next work if is idling
    }
}
