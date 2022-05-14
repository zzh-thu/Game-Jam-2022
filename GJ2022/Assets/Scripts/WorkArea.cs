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

    private float _efficiency;
    private bool _isWorking = false;
    private float _progress;
    
    public float neededProgress;
    public int[] neededRaw = new int[2]; // not used in WorkType == Raw
    public WorkType workType;
    public int workTypeNumber;  // not used in WorkType == Robot
    public int price;  // only used in WorkType == Production
    public bool isTurnedOn = true;

    public void RemoveRobot(Robot robot)
    {
        // TODO: buff of robots
        var inventory = Inventory.GetInventory();
        switch (workType)
        {
            case WorkType.Raw:
                _efficiency -= robot.rawEfficiencies[workTypeNumber];
                inventory.RawEfficiencies[workTypeNumber] = _efficiency;
                break;
            case WorkType.Robot:
                _efficiency -= robot.robotEfficiency;
                inventory.RobotEfficiency = _efficiency;
                break;
            case WorkType.Production:
                _efficiency -= robot.productionEfficiencies[workTypeNumber];
                inventory.ProductionEfficiencies[workTypeNumber] = _efficiency;
                break;
        }
        
    }

    public void PutRobot(int x, int y)
    {
        var robot = Inventory.GetInventory().FetchBufferedRobot();
        robot.x = x;
        robot.y = y;
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
    }

    public void TurnOnOrOff()
    {
        isTurnedOn = !isTurnedOn;
    }
    
    private void _TryProduce()
    {
        var inventory = Inventory.GetInventory();
        for (int i = 0; i < neededRaw.Length; ++i)  // check whether each raw materials is enough
        {
            if (inventory.RawAmounts[i] < neededRaw[i]) return;
        }

        for (int i = 0; i < neededRaw.Length; ++i)  // deduct materials from inventory
        {
            inventory.RawAmounts[i] -= neededRaw[i];
        }

        _isWorking = true;
    }


    // Start is called before the first frame update
    void Start()
    {
        // TODO: buff of robots
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
    }

    // Update is called once per frame
    void Update()
    {
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
                    inventory.RawAmounts[workTypeNumber] += 1;
                    break;
                case WorkType.Robot:
                    inventory.RobotAmount += 1;
                    break;
                case WorkType.Production:
                    inventory.Score += price;
                    break;
            }
            _progress = 0;  // change the state to idle
            _isWorking = false;
        }
        
        if (!_isWorking) _TryProduce();  // try to start next work if is idling
    }
}
