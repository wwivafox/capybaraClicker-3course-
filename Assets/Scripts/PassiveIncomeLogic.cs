using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PassiveIncomeLogic : MonoBehaviour
{
    public static int passiveIncome = 0;
    public static float updateInterval = 30f;

    private float _timer;

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= updateInterval)
        {
            _timer = 0f;
            ClickingLogic.score += passiveIncome;
            ClickingLogic.OnScoreChanged.Invoke();
        }
    }

    public static void SetUpdateInterval(float newInterval)
    {
        updateInterval = newInterval;
    }
}