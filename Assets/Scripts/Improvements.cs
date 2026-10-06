using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SQLite4Unity3d;
using System.Linq;
using System.Collections;

public class Improvements : MonoBehaviour
{
    public TMP_Text clickTitleText;
    public TMP_Text clickCostText;
    public Button clickBuyButton;

    public TMP_Text passiveTitleText;
    public TMP_Text passiveCostText;
    public Button passiveBuyButton;

    public TMP_Text intervalTitleText;
    public TMP_Text intervalCostText;
    public Button intervalBuyButton;

    private User currentUser;
    private Upgrade[] clickUpgrades;
    private Upgrade[] passiveUpgrades;
    private Upgrade[] intervalUpgrades;
    private ClickingLogic clickingLogic;
    private SQLiteConnection DB;

    private void OnEnable()
    {
        StartCoroutine(InitializeWhenReady());
    }

    private IEnumerator InitializeWhenReady()
    {
      
        while (DatabaseManager.Instance == null)
        {
            yield return null;
        }
        while (DatabaseManager.Instance.CurrentUser == null)
        {
            yield return null;
        }

        InitializeComponents();
    }

    private void InitializeComponents()
    {
        clickingLogic = FindObjectOfType<ClickingLogic>();
        DB = DatabaseManager.Instance.DB;
        currentUser = DatabaseManager.Instance.CurrentUser;

        

        LoadUpgrades();
        SetupButtonListeners();
        UpdateUI();

        ClickingLogic.OnScoreChanged.AddListener(UpdateUI);
    }

    private void LoadUpgrades()
    {
        clickUpgrades = DB.Table<Upgrade>()
                         .Where(u => u.type == "click")
                         .OrderBy(u => u.level)
                         .ToArray();

        passiveUpgrades = DB.Table<Upgrade>()
                          .Where(u => u.type == "passive")
                          .OrderBy(u => u.level)
                          .ToArray();

        intervalUpgrades = DB.Table<Upgrade>()
                           .Where(u => u.type == "interval")
                           .OrderBy(u => u.level)
                           .ToArray();

       
    }

    private void SetupButtonListeners()
    {
        clickBuyButton.onClick.AddListener(BuyClickUpgrade);
        passiveBuyButton.onClick.AddListener(BuyPassiveUpgrade);
        intervalBuyButton.onClick.AddListener(BuyIntervalUpgrade);
    }

    private void UpdateUI()
    {
        UpdateClickUI();
        UpdatePassiveUI();
        UpdateIntervalUI();
    }

    private void UpdateClickUI()
    {
        int nextLevel = currentUser.click_level + 1;

        if (nextLevel <= clickUpgrades.Length)
        {
            var upgrade = clickUpgrades[nextLevel - 1];
            clickTitleText.text = upgrade.name;
            clickCostText.text = $"{upgrade.price} Капибаксов";
            clickBuyButton.interactable = ClickingLogic.score >= upgrade.price;
        }
        else
        {
            clickTitleText.text = "Макс. уровень!";
            clickCostText.text = "";
            clickBuyButton.interactable = false;
        }
    }

    private void BuyClickUpgrade()
    {
        int nextLevel = currentUser.click_level + 1;
        if (nextLevel > clickUpgrades.Length) return;

        var upgrade = clickUpgrades[nextLevel - 1];

        if (ClickingLogic.score >= upgrade.price)
        {
            clickingLogic.AddScore(-upgrade.price);
            currentUser.click_level = nextLevel;
            clickingLogic.SetClickValue((int)upgrade.value);

            DB.Update(currentUser);
            UpdateUI();
        }
    }

    private void UpdatePassiveUI()
    {
        int nextLevel = currentUser.passive_level + 1;

        if (nextLevel <= passiveUpgrades.Length)
        {
            var upgrade = passiveUpgrades[nextLevel - 1];
            passiveTitleText.text = upgrade.name;
            passiveCostText.text = $"{upgrade.price} Капибаксов";
            passiveBuyButton.interactable = ClickingLogic.score >= upgrade.price;
        }
        else
        {
            passiveTitleText.text = "Макс. уровень!";
            passiveCostText.text = "";
            passiveBuyButton.interactable = false;
        }
    }

    private void BuyPassiveUpgrade()
    {
        int nextLevel = currentUser.passive_level + 1;
        if (nextLevel > passiveUpgrades.Length) return;

        var upgrade = passiveUpgrades[nextLevel - 1];

        if (ClickingLogic.score >= upgrade.price)
        {
            clickingLogic.AddScore(-upgrade.price); 
            currentUser.passive_level = nextLevel;
            currentUser.passive_income = (int)upgrade.value;

            DB.Update(currentUser);
            UpdateUI();
        }
    }

    private void UpdateIntervalUI()
    {
        if (currentUser.passive_level == 0)
        {
            intervalTitleText.text = "Сначала купите пассивный доход";
            intervalCostText.text = "";
            intervalBuyButton.interactable = false;
            return;
        }

        int nextLevel = currentUser.interval_level + 1;

        if (nextLevel <= intervalUpgrades.Length)
        {
            var upgrade = intervalUpgrades[nextLevel - 1];
            intervalTitleText.text = upgrade.name;
            intervalCostText.text = $"{upgrade.price} Капибаксов";
            intervalBuyButton.interactable = ClickingLogic.score >= upgrade.price;
        }
        else
        {
            intervalTitleText.text = "Макс. уровень!";
            intervalCostText.text = "";
            intervalBuyButton.interactable = false;
        }
    }

    private void BuyIntervalUpgrade()
    {
        if (currentUser.passive_level == 0) return;

        int nextLevel = currentUser.interval_level + 1;
        if (nextLevel > intervalUpgrades.Length) return;

        var upgrade = intervalUpgrades[nextLevel - 1];

        if (ClickingLogic.score >= upgrade.price)
        {
            clickingLogic.AddScore(-upgrade.price); 
            currentUser.interval_level = nextLevel;
            currentUser.income_interval = upgrade.value;

            DB.Update(currentUser);
            UpdateUI();
        }
    }

    private void OnDisable()
    {
        ClickingLogic.OnScoreChanged.RemoveListener(UpdateUI);
    }
}