using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using SQLite4Unity3d;

public class AccessoriesLogic : MonoBehaviour
{
    public ClickingLogic clickingLogic;
    public Animator capybaraAnimator;
    public RuntimeAnimatorController defaultAnimator;
    public RuntimeAnimatorController[] accessoryAnimators;
    public Button[] buttons;
    public TMP_Text[] descriptions;
    public List<Accessory> accessories;

    private int currentAccessoryIndex = 7;
    public Color defaultColor = Color.white;
    public Color appliedColor = Color.blue;

    private int userId
    {
        get
        {
            if (DatabaseManager.Instance != null && DatabaseManager.Instance.CurrentUser != null)
            {
                return DatabaseManager.Instance.CurrentUser.id;
            }
            return 1;
        }
    }

    void Start()
    {
        //if (clickingLogic == null)
        //{
        //    clickingLogic = FindObjectOfType<ClickingLogic>();
        //    if (clickingLogic == null) Debug.LogError("ClickingLogic не найден на сцене");
        //}

        //if (DatabaseManager.Instance == null)
        //{
        //    Debug.LogError("DatabaseManager не инициализирован");
        //    return;
        //}

        InitializeAccessories();
        SetupButtons();
        ClickingLogic.OnScoreChanged.AddListener(UpdateButtons);
        UpdateButtons();
    }

    private void InitializeAccessories()
    {
        
        accessories = DatabaseManager.Instance.DB.Table<Accessory>().OrderBy(a => a.id).ToList();

        //if (accessories.Count == 0)
        //{
        //    Debug.LogError("Нет аксессуаров в базе данных!");
        //    return;
        //}

        //Debug.Log("Загружены аксессуары:");
        //foreach (var acc in accessories)
        //{
        //    Debug.Log($"ID: {acc.id}, Price: {acc.price}");
        //}
    }

    private void SetupButtons()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnButtonClick(index));
            buttons[i].interactable = true;
        }
        UpdateButtons();
    }

    void OnButtonClick(int index)
    {
        var db = DatabaseManager.Instance.DB;
        var accessoryId = accessories[index].id;

        var accessory = db.Find<Accessory>(accessoryId);
        if (accessory == null)
        {
            Debug.LogWarning($"Аксессуар с ID {accessoryId} не найден в БД");
            return;
        }

        var item = db.Table<UserItem>()
                   .FirstOrDefault(x => x.user_id == userId &&
                                      x.item_type == "accessory" &&
                                      x.item_id == accessoryId);

        if (item != null)
        {
            if (item.is_equipped == 1)
            {
                RemoveAccessory();
            }
            else
            {
                ApplyAccessory(accessoryId);
            }
        }
        else
        {
            if (ClickingLogic.score >= accessories[index].price)
            {
                PurchaseAccessory(accessoryId);
            }
            else
            {
                Debug.Log("Недостаточно средств для покупки");
            }
        }
    }

    public void ApplyAccessory(int accessoryId)
    {
        int index = accessories.FindIndex(a => a.id == accessoryId);


        var db = DatabaseManager.Instance.DB;
        db.Execute("UPDATE UserItem SET is_equipped = 0 WHERE user_id = ? AND item_type = ?",
                 userId, "accessory");
        db.Execute("UPDATE UserItem SET is_equipped = 1 WHERE user_id = ? AND item_type = ? AND item_id = ?",
                 userId, "accessory", accessoryId);

        currentAccessoryIndex = index;
        capybaraAnimator.runtimeAnimatorController = accessoryAnimators[index]; 
        UpdateButtons();
    }

    void PurchaseAccessory(int accessoryId)
    {
        int index = accessories.FindIndex(a => a.id == accessoryId);

        var accessory = accessories[index];
        if (ClickingLogic.score >= accessory.price)
        {
            ClickingLogic.score -= accessory.price;
            var db = DatabaseManager.Instance.DB;
            db.Insert(new UserItem
            {
                user_id = userId,
                item_type = "accessory",
                item_id = accessory.id,
                is_equipped = 0 
            });
            ClickingLogic.OnScoreChanged.Invoke();
            UpdateButtons();
        }
    }

    void RemoveAccessory()
    {
        var db = DatabaseManager.Instance.DB;
        db.Execute("UPDATE UserItem SET is_equipped = 0 WHERE user_id = ? AND item_type = ?",
                 userId, "accessory");

        var defaultItem = db.Table<UserItem>()
                 .FirstOrDefault(x => x.user_id == userId &&
                                    x.item_type == "accessory" &&
                                    x.item_id == 7);

        if (defaultItem == null)
        {
            db.Insert(new UserItem
            {
                user_id = userId,
                item_type = "accessory",
                item_id = 7,
                is_equipped = 1
            });
        }
        else
        {
            db.Execute("UPDATE UserItem SET is_equipped = 1 WHERE user_id = ? AND item_type = ? AND item_id = ?",
                     userId, "accessory", -1);
        }

        currentAccessoryIndex = 7;
        capybaraAnimator.runtimeAnimatorController = defaultAnimator;
        UpdateButtons();
    }

    void UpdateButtons()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            UpdateButtonState(i);
        }
    }

    void UpdateButtonState(int index)
    {
        var db = DatabaseManager.Instance.DB;
        var accessoryId = accessories[index].id;
        var item = db.Table<UserItem>()
                   .FirstOrDefault(x => x.user_id == userId &&
                                      x.item_type == "accessory" &&
                                      x.item_id == accessoryId);

        if (buttons[index] == null)
        {
            Debug.LogError($"Кнопка с индексом {index} не назначена");
            return;
        }

        TMP_Text buttonText = buttons[index].GetComponentInChildren<TMP_Text>();
        Image buttonImage = buttons[index].GetComponent<Image>();
        TMP_Text descriptionText = descriptions[index];

        if (buttonText == null || buttonImage == null || descriptionText == null)
        {
            Debug.LogError($"Не назначены UI элементы для кнопки {index}");
            return;
        }

        var isDefaultEquipped = db.Table<UserItem>()
                                .Any(x => x.user_id == userId &&
                                         x.item_type == "accessory" &&
                                         x.item_id == 7 &&
                                         x.is_equipped == 1);


            bool canAfford = ClickingLogic.score >= accessories[index].price;
            buttons[index].interactable = item != null || canAfford;

            if (item != null)
            {
                descriptionText.text = "Куплено";
                buttons[index].interactable = true;

                if (isDefaultEquipped)
                {
                    buttonText.text = "Применить";
                    buttonImage.color = defaultColor;
                }
                else if (item.is_equipped == 1)
                {
                    buttonText.text = "Применено";
                    buttonImage.color = appliedColor;
                }
                else
                {
                    buttonText.text = "Применить";
                    buttonImage.color = defaultColor;
                }
            }
            else
            {
                descriptionText.text = $"{accessories[index].price} Капибаксов";
                buttonText.text = "Купить";
                buttonImage.color = defaultColor;
            }
     
    }

    void OnDestroy()
    {
        ClickingLogic.OnScoreChanged.RemoveListener(UpdateButtons);
    }
}