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
    public int xNum;
    public int yNum;
    private Robot[][] _robots;

    // state
    private float _efficiency;
    private bool _isWorking;
    private float _progress;
    public bool isTurnedOn;

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

    private void _UpdateRobotsStates()
    {
        for (int i = 0; i < xNum; ++i)  // reset debuff num to 0
        {
            for (int j = 0; j < yNum; ++j)
            {
                _robots[i][j].debuffNum = 0;
            }
        }

        for (int i = 0; i < xNum; ++i)  // calculate debuff num
        {
            for (int j = 0; j < yNum; ++j)
            {
                if (_robots[i][j].patience <= 0)  // if runs out of patience
                {
                    for (int m = -1; m <= 1; ++m)
                    {
                        for (int n = -1; n <= 1; ++n)
                        {
                            // get all robots around it
                            int ii = i + m, jj = j + n;
                            if (0 <= ii && ii < xNum && 0 <= jj && jj < yNum && (ii != i || jj != j))
                            {
                                ++_robots[ii][jj].debuffNum;
                            }
                        }
                    }
                }
            }
        }

        _efficiency = 0;
        for (int i = 0; i < xNum; ++i)  // calculate the efficiency
        {
            for (int j = 0; j < yNum; ++j)
            {
                _efficiency += _robots[i][j].GetEfficiency();
            }
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

    public void PutRobot(int x, int y)
    {
        var robot = Inventory.GetInventory().FetchBufferedRobot();
        // TODO: buff of robots
        switch (workType)
        {
            case WorkType.Raw:
                _efficiency += robot.rawEfficiencies[workTypeNumber];
                break;
            case WorkType.Robot:
                _efficiency += robot.robotEfficiency;
                break;
            case WorkType.Production:
                _efficiency += robot.productionEfficiencies[workTypeNumber];
                break;
        }
        _InformEfficiency();
    }

    public Robot FindRecycledAgent(int x, int y)
    {
        // TODO
        // find by distance
        // set its state
        // navigate it to (x, y)
    }

    public void ReleaseRecycledAgent(Robot recycledRobot)
    {
        // TODO:
        // navigate agent to (x, y)
        // set its state
        // release recycledRobot
    }

    public void TurnOnOrOff()
    {
        isTurnedOn = !isTurnedOn;
    }
    
    private void _TryProduce()
    {
        var inventory = Inventory.GetInventory();
        for (int i = 0; i < inputRaw.Length; ++i)  // check whether each raw materials is enough
            if (inventory.rawAmounts[i] < inputRaw[i]) return;

        for (int i = 0; i < inputRaw.Length; ++i)  // deduct materials from inventory
            inventory.rawAmounts[i] -= inputRaw[i];

        _isWorking = true;
    }


    // Start is called before the first frame update
    void Start()
    {
        // TODO: initiate
        _robots = new Robot[][xNum];
        for (int i = 0; i < xNum; ++i)
            _robots[i] = new Robot[yNum];
        inputRaw = new int[Inventory.numOfRaw];
        isTurnedOn = true;

        // TODO
        foreach (var robot in GetComponentsInChildren<Robot>())  // calculate the initial efficiency
        {
            switch (workType)
            {
                case WorkType.Raw:
                    _efficiency += robot.rawEfficiencies[workTypeNumber];
                    break;
                case WorkType.Robot:
                    _efficiency += robot.robotEfficiency;
                    break;
                case WorkType.Production:
                    _efficiency += robot.productionEfficiencies[workTypeNumber];
                    break;
            }
        }
        _InformEfficiency();
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
        
        _UpdateRobotsStates();
        
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
                    inventory.score += price;
                    break;
            }
            _progress = 0;  // change the state to idle
            _isWorking = false;
        }
        
        if (!_isWorking) _TryProduce();  // try to start next work if is idling
    }
}
