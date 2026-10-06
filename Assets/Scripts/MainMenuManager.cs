using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SQLite4Unity3d;


public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel; 
    public GameObject profilePanel; 
    public GameObject gamePanel; 
    public ClickingLogic clickingLogic; 
    public Button playButton;
    public bool isProfileSelected;

    public void OnProfileDeselected()
    {
        isProfileSelected = false;
        UpdatePlayButtonState();
        Debug.Log("Профиль не выбран, кнопка 'Играть' заблокирована");
    }

    public void OnProfileSelected()
    {
        isProfileSelected = true;
        UpdatePlayButtonState();
        Debug.Log("Профиль выбран, кнопка 'Играть' разблокирована");
    }

    private void UpdatePlayButtonState()
    {
        if (playButton != null)
        {
            playButton.interactable = isProfileSelected;

            var colors = playButton.colors;
            colors.normalColor = isProfileSelected ? Color.white : Color.gray;
            playButton.colors = colors;
        }
    }

    private void Start()
    {
       
        ShowMainMenu(); 
        UpdatePlayButtonState();
    }

  

    public void ShowMainMenu()
    {
       
        mainMenuPanel.SetActive(true);
        profilePanel.SetActive(false);
        gamePanel.SetActive(false);

        Debug.Log("Главное меню активно");
    }

    public void ShowProfileMenu()
    {

        profilePanel.SetActive(true);

        mainMenuPanel.SetActive(false);
        gamePanel.SetActive(false);

        Debug.Log("Меню профилей активно");
    }

    public void ShowGamePanel()
    {
        gamePanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        profilePanel.SetActive(false);

        Debug.Log("Игровая панель активна");

   
        if (clickingLogic != null)
            clickingLogic.LoadScoreFromDatabase();

        if (DatabaseManager.Instance != null && DatabaseManager.Instance.CurrentUser != null)
        {
            PlayerDataLoader.LoadPlayerData();
        }
        else
        {
            Debug.LogWarning("Не удалось загрузить данные игрока: DatabaseManager не инициализирован");
        }
    }

    public void BackToMenu()
    {

        ShowMainMenu();
        Debug.Log("Возврат в главное меню");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}