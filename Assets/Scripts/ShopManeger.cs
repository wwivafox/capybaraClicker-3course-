using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManeger : MonoBehaviour
{
    public GameObject shopWindow;
    private MenuManager menuManager;

    public GameObject improvementsPanel;
    public GameObject accessoriesPanel;
    public GameObject backgroundPanel;

    public GameObject Capybara;
    private Image capybaraImage;

    public Button improvementsButton;
    public Button accessoriesButton;
    public Button backgroundButton;

    public static bool IsShopOpen { get; private set; }

    void Start()
    {
        menuManager = FindObjectOfType<MenuManager>();
        shopWindow.SetActive(false);

        if (Capybara != null)
        {
            capybaraImage = Capybara.GetComponent<Image>();
            if (capybaraImage == null)
            {
                Debug.LogError("Ошибка: У объекта Capybara отсутствует компонент Image!");
            }
        }
        else
        {
            Debug.LogError("Ошибка: Объект Capybara не привязан в Inspector!");
        }

        improvementsButton.onClick.AddListener(ShowImprovementsPanel);
        accessoriesButton.onClick.AddListener(ShowAccessoriesPanel);
        backgroundButton.onClick.AddListener(ShowBackgroundPanel);
    }

    public void ShowShop()
    {
        IsShopOpen = true;
        shopWindow.SetActive(true);
        SetActivePanel(improvementsPanel);
        UpdateButtonStates();
       
        if (capybaraImage != null)
        {
            capybaraImage.raycastTarget = false;
        }
    }

    public void CloseShop()
    {
        IsShopOpen = false;
        shopWindow.SetActive(false);
        menuManager.OnShopClosed();

        if (capybaraImage != null)
        {
            capybaraImage.raycastTarget = true;
        }
    }

    public void ShowImprovementsPanel()
    {
        if (!improvementsPanel.activeSelf)
        {
            SetActivePanel(improvementsPanel);
            UpdateButtonStates();
        }
    }

    public void ShowAccessoriesPanel()
    {
        if (!accessoriesPanel.activeSelf)
        {
            SetActivePanel(accessoriesPanel);
            UpdateButtonStates();
        }
    }

    public void ShowBackgroundPanel()
    {
        if (!backgroundPanel.activeSelf)
        {
            SetActivePanel(backgroundPanel);
            UpdateButtonStates();
        }
    }

    private void SetActivePanel(GameObject panel)
    {
        improvementsPanel.SetActive(false);
        accessoriesPanel.SetActive(false);
        backgroundPanel.SetActive(false);

        panel.SetActive(true);
    }

    private void UpdateButtonStates()
    {
        improvementsButton.interactable = !improvementsPanel.activeSelf;
        accessoriesButton.interactable = !accessoriesPanel.activeSelf;
        backgroundButton.interactable = !backgroundPanel.activeSelf;
    }
}