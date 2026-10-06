using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{

    public ShopManeger shopManeger;
    public MainMenuManager mainMenuManager; 

    public Button shopButton;
    public Button homeButton;
    public Button img;

    private void Start()
    {
        shopButton.onClick.AddListener(OpenShop);
        homeButton.onClick.AddListener(ReturnToMainMenu); 
    }
  

    public void OpenShop()
    {
        shopManeger.ShowShop();
        SetButtonsInteractable(false);
    }

    public void OnShopClosed()
    {
        SetButtonsInteractable(true);
    }

    public void ReturnToMainMenu()
    {
        if (mainMenuManager != null)
        {
            mainMenuManager.BackToMenu();
        }
        else
        {
            Debug.LogError("MainMenuManager не назначен!");
        }
    }


    private void SetButtonsInteractable(bool state)
    {
        shopButton.interactable = state;
        img.interactable = state;
        homeButton.interactable = state; 
    }
}