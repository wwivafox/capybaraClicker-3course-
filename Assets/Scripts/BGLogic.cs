using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using SQLite4Unity3d;

public class BGLogic : MonoBehaviour
{
    public ClickingLogic clickingLogic;
    public Image backgroundImage;
    public Sprite[] backgroundSprites;
    public Button[] buttons;
    public TMP_Text[] descriptions;
    private List<Background> backgrounds;

    private int currentBackgroundIndex = -1;
    private int userId => DatabaseManager.Instance.CurrentUser?.id ?? 1;

    void Start()
    {
        ClickingLogic.OnScoreChanged.AddListener(UpdateButtons);

        if (DatabaseManager.Instance == null || DatabaseManager.Instance.DB == null)
        {
            Debug.LogError("DatabaseManager не инициализирован");
            return;
        }

        var db = DatabaseManager.Instance.DB;
        backgrounds = db.Table<Background>().OrderBy(b => b.id).ToList();


        if (buttons.Length != backgrounds.Count || backgroundSprites.Length != backgrounds.Count || descriptions.Length != backgrounds.Count)
        {
            Debug.LogError($"Несоответствие количества элементов! Фоны: {backgrounds.Count}, Кнопки: {buttons.Length}, Спрайты: {backgroundSprites.Length}, Описания: {descriptions.Length}");
            return;
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnButtonClick(index));
            descriptions[i].text = $"{backgrounds[i].price} Капибакс";
        }

        var equippedBg = db.Table<UserItem>()
                          .FirstOrDefault(x => x.user_id == userId &&
                                             x.item_type == "background" &&
                                             x.is_equipped == 1);
        if (equippedBg != null)
        {
            ApplyBackground(equippedBg.item_id);
        }

        UpdateButtons();
    }

    void OnDestroy()
    {
        ClickingLogic.OnScoreChanged.RemoveListener(UpdateButtons);
    }

    void OnButtonClick(int index)
    {
        if (index < 0 || index >= backgrounds.Count)
        {
            Debug.LogError($"Неверный индекс: {index}");
            return;
        }

        var db = DatabaseManager.Instance.DB;
        var item = db.Table<UserItem>()
                     .FirstOrDefault(x => x.user_id == userId &&
                                        x.item_type == "background" &&
                                        x.item_id == backgrounds[index].id);

        if (item != null)
        {
            ApplyBackground(backgrounds[index].id);
        }
        else
        {
            PurchaseBackground(backgrounds[index].id);
        }
    }

    void PurchaseBackground(int backgroundId)
    {
        int index = backgrounds.FindIndex(b => b.id == backgroundId);
        if (index == -1)
        {
            Debug.LogWarning($"Фон с ID {backgroundId} не найден");
            return;
        }

        if (ClickingLogic.score >= backgrounds[index].price)
        {
            ClickingLogic.score -= backgrounds[index].price;
            var db = DatabaseManager.Instance.DB;
            var existingItem = db.Table<UserItem>()
                                .FirstOrDefault(x => x.user_id == userId &&
                                                   x.item_type == "background" &&
                                                   x.item_id == backgroundId);

            if (existingItem == null)
            {
                db.Insert(new UserItem
                {
                    user_id = userId,
                    item_type = "background",
                    item_id = backgroundId,
                    is_equipped = 0
                });
            }
            else
            {
                Debug.LogWarning($"Фон {backgroundId} уже куплен, но отображался как некупленный");
            }

            ClickingLogic.OnScoreChanged.Invoke();
            UpdateButtons();
        }
    }

    public void ApplyBackground(int backgroundId)
    {
        int index = backgrounds.FindIndex(b => b.id == backgroundId);
        if (index == -1)
        {
            Debug.LogWarning($"Фон с ID {backgroundId} не найден");
            return;
        }

        var db = DatabaseManager.Instance.DB;
        db.Execute("UPDATE UserItem SET is_equipped = 0 WHERE user_id = ? AND item_type = ?",
                 userId, "background");
        db.Execute("UPDATE UserItem SET is_equipped = 1 WHERE user_id = ? AND item_type = ? AND item_id = ?",
                 userId, "background", backgroundId);

        currentBackgroundIndex = index;
        backgroundImage.sprite = backgroundSprites[index];
        UpdateButtons();
    }

    void UpdateButtons()
    {
        var db = DatabaseManager.Instance.DB;
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i >= backgrounds.Count)
            {
                Debug.LogError($"Индекс {i} выходит за пределы списка фонов");
                continue;
            }

            var item = db.Table<UserItem>()
                         .FirstOrDefault(x => x.user_id == userId &&
                                            x.item_type == "background" &&
                                            x.item_id == backgrounds[i].id);

            TMP_Text buttonText = buttons[i].GetComponentInChildren<TMP_Text>();
            if (buttonText == null || descriptions[i] == null)
            {
                Debug.LogError($"Не найден текст кнопки или описание для индекса {i}");
                continue;
            }

            bool canAfford = ClickingLogic.score >= backgrounds[i].price;

            if (item != null)
            {
                if (item.is_equipped == 1)
                {
                    descriptions[i].text = "Куплено";
                    buttonText.text = "Применено";
                    buttons[i].interactable = false;
                }
                else
                {
                    descriptions[i].text = "Куплено";
                    buttonText.text = "Применить";
                    buttons[i].interactable = true;
                }
            }
            else
            {
                descriptions[i].text = $"{backgrounds[i].price} Капибакс";
                buttonText.text = "Купить";
                buttons[i].interactable = canAfford;
            }
        }
    }
}