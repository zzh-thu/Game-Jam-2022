using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITextDataFetcher : MonoBehaviour
{
    public enum DataType
    {
        Amount,
        Efficiency,
        Score,
        ScoreCondition,
        TaskFinalCondition,
        Time,
        RobotEfficiency,
        RobotBoundEfficiency,
        RobotUsedTime,
        RobotPrice,
    }

    public DataType dataType;
    public WorkArea.WorkType workType;
    public int workTypeNum;

    private Text _text;
    
    void Start()
    {
        _text = gameObject.GetComponent<Text>();
    }

    private String _GetNeededData()
    {
        Robot robot = null; // TODO
        var inventory = Inventory.GetInventory();
        switch (dataType)
        {
            case DataType.Amount:
                switch (workType)
                {
                    case WorkArea.WorkType.Raw:
                        return inventory.rawAmounts[workTypeNum].ToString();
                    case WorkArea.WorkType.Robot:
                        return inventory.robotAmount.ToString();
                    case WorkArea.WorkType.Production:
                        return inventory.productionAmount.ToString();
                    case WorkArea.WorkType.Emergency:
                        return inventory.specialAmount[workTypeNum].ToString();
                }
                break;
            case DataType.Efficiency:
                switch (workType)
                {
                    case WorkArea.WorkType.Raw:
                        return inventory.rawEfficiencies[workTypeNum].ToString();
                    case WorkArea.WorkType.Robot:
                        return inventory.robotEfficiency.ToString();
                    case WorkArea.WorkType.Production:
                        return inventory.productionEfficiencies[workTypeNum].ToString();
                }
                break;
            case DataType.Score:
                return inventory.score.ToString();
            case DataType.ScoreCondition:
                return $"{inventory.score}/{inventory.finalTargetScore}";
            case DataType.TaskFinalCondition:
                return $"{inventory.accomplishedTaskNum}/{Inventory.taskNum}";
            case DataType.Time:
                return $"{(int)(inventory.time / 60)}:{(int)(inventory.time % 60)}";
            case DataType.RobotEfficiency:
                switch (workType)
                {
                    case WorkArea.WorkType.Raw:
                        return robot.rawEfficiencies[workTypeNum].ToString();
                    case WorkArea.WorkType.Robot:
                        return robot.robotEfficiency.ToString();
                    case WorkArea.WorkType.Production:
                        return robot.productionEfficiencies[workTypeNum].ToString();
                }
                break;
            case DataType.RobotBoundEfficiency:
                return robot.GetEfficiency().ToString();
            case DataType.RobotUsedTime:
                return ((int)(robot.age)).ToString();
            case DataType.RobotPrice:
                return robot.price.ToString();
            default:
                return "";
        }

        return "";
    }
    
    void Update()
    {
        _text.text = _GetNeededData();
    }
}
