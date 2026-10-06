using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using SQLite4Unity3d;

public class ClickingLogic : MonoBehaviour
{
    public AnimationScript capybaraScript;
    public static int score;
    public TMP_Text scoreText;
    public int clickValue = 1;
    public static UnityEvent OnScoreChanged = new UnityEvent();
    public float idleThreshold = 5f;
    private float lastClickTime;
    private bool animationTriggered;
    public static ClickingLogic Instance { get; private set; }

    void Start()
    {

        score = DatabaseManager.Instance.CurrentUser?.score ?? 0;
        OnScoreChanged.AddListener(UpdateScoreText);
        lastClickTime = Time.time;
        UpdateScoreText();
    }

    public void UpdateScoreText()
    {
        scoreText.text = score.ToString();
    }

    public void AddScore(int amount)
    {
        score += amount;

        if (DatabaseManager.Instance.CurrentUser != null)
        {
            DatabaseManager.Instance.CurrentUser.score = score;
            DatabaseManager.Instance.DB.Update(DatabaseManager.Instance.CurrentUser);
        }

        OnScoreChanged.Invoke();
        UpdateScoreText();
    }

    public void SetClickValue(int newValue)
    {
        clickValue = newValue;
    }

    public void Clicked()
    {
        if (ShopManeger.IsShopOpen) return;

        score += clickValue;

        if (DatabaseManager.Instance.CurrentUser != null)
        {
            DatabaseManager.Instance.CurrentUser.score = score;
            DatabaseManager.Instance.DB.Update(DatabaseManager.Instance.CurrentUser);
        }

        OnScoreChanged.Invoke();
        lastClickTime = Time.time;
        animationTriggered = false;

        SoundManager.instance.PlayClickSound();

        if (capybaraScript != null)
        {
            capybaraScript.OnCapybaraClick();
        }
        else
        {
            Debug.LogError("Ошибка: capybaraScript не назначен!");
        }
        UpdateScoreText();
    }

    void Update()
    {
        if (animationTriggered) return;
        float idleTime = Time.time - lastClickTime;

        if (Time.time - lastClickTime >= idleThreshold)
        {
            capybaraScript.EnableRandomAnimation();
            capybaraScript.PlayRandomAnimation();
            lastClickTime = Time.time;
        }
    }

    public void LoadScoreFromDatabase()
    {
        if (DatabaseManager.Instance.CurrentUser != null)
        {
            score = DatabaseManager.Instance.CurrentUser.score;
            UpdateScoreText();
        }
    }
}