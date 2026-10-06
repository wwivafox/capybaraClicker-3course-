using UnityEngine;
using SQLite4Unity3d;
using System.Linq;

public static class PlayerDataLoader
{
    public static void LoadPlayerData()
    {
        if (DatabaseManager.Instance == null || DatabaseManager.Instance.CurrentUser == null)
        {
            Debug.LogWarning("DatabaseManager или CurrentUser не инициализированы");
            return;
        }

        var user = DatabaseManager.Instance.CurrentUser;
        var db = DatabaseManager.Instance.DB;

        InitializeDefaultPlayerData(user, db);

        LoadImprovements(user, db);
        LoadAccessories();
        LoadBackgrounds();

        //Debug.Log($"Данные игрока {user.username} загружены. Уровни: {user.click_level}/{user.passive_level}/{user.interval_level}");
    }

    private static void InitializeDefaultPlayerData(User user, SQLiteConnection db)
    {
 
        bool isNewPlayer = !db.Table<UserItem>().Any(x => x.user_id == user.id);

        if (isNewPlayer)
        {
            //Debug.Log($"Инициализация нового игрока: {user.username}");

            var firstBackground = db.Table<Background>().OrderBy(b => b.id).FirstOrDefault();
            if (firstBackground != null)
            {
                db.Insert(new UserItem
                {
                    user_id = user.id,
                    item_type = "background",
                    item_id = firstBackground.id,
                    is_equipped = 1
                });
            }

            user.click_level = 0;
            user.passive_level = 0;
            user.interval_level = 0;
            db.Update(user);
        }
    }

    private static void LoadImprovements(User user, SQLiteConnection db)
    {
        ClickingLogic clickingLogic = Object.FindFirstObjectByType<ClickingLogic>();
        if (clickingLogic == null)
        {
            Debug.LogWarning("ClickingLogic не найден");
            return;
        }

        var clickUpgrade = db.Table<Upgrade>()
                           .Where(u => u.type == "click" && u.level == user.click_level)
                           .FirstOrDefault();

        if (clickUpgrade != null)
        {
            clickingLogic.SetClickValue((int)clickUpgrade.value);
        }

        if (user.passive_level > 0)
        {
            var passiveUpgrade = db.Table<Upgrade>()
                                 .Where(u => u.type == "passive" && u.level == user.passive_level)
                                 .FirstOrDefault();

            if (passiveUpgrade != null)
            {
                PassiveIncomeLogic.passiveIncome = (int)passiveUpgrade.value;
            }
        }

        if (user.interval_level > 0)
        {
            var intervalUpgrade = db.Table<Upgrade>()
                                  .Where(u => u.type == "interval" && u.level == user.interval_level)
                                  .FirstOrDefault();

            if (intervalUpgrade != null)
            {
                PassiveIncomeLogic.SetUpdateInterval(intervalUpgrade.value);
            }
        }
    }

    private static void LoadAccessories()
    {
        AccessoriesLogic accessoriesLogic = Object.FindFirstObjectByType<AccessoriesLogic>();
        if (accessoriesLogic != null)
        {
            
            var equippedAccessory = DatabaseManager.Instance.GetEquippedItem("accessory");
            if (equippedAccessory.HasValue)
            {
                accessoriesLogic.ApplyAccessory(equippedAccessory.Value);
            }
            else
            {
                accessoriesLogic.ApplyAccessory(-1);

                
                if (accessoriesLogic.defaultAnimator != null)
                {
                    accessoriesLogic.capybaraAnimator.runtimeAnimatorController =
                        accessoriesLogic.defaultAnimator;
                }
            }
        }
    }
    private static void LoadBackgrounds()
    {
        BGLogic bgLogic = Object.FindFirstObjectByType<BGLogic>();
        if (bgLogic != null)
        {
            var equippedBackground = DatabaseManager.Instance.GetEquippedItem("background");
            if (equippedBackground.HasValue)
            {
                bgLogic.ApplyBackground(equippedBackground.Value);
            }
            else
            {
                
                var firstBackground = DatabaseManager.Instance.DB.Table<Background>()
                    .OrderBy(b => b.id)
                    .FirstOrDefault();

                if (firstBackground != null)
                {
                    bgLogic.ApplyBackground(firstBackground.id);
                }
            }
        }
    }
}