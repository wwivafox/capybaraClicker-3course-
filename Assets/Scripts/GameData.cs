using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameData : MonoBehaviour
{
    public static GameData Instance;

    public int currentUserId;
    public int score;
    public int clickLevel;
    public int passiveLevel;
    public List<int> ownedAccessories = new List<int>();
    public List<int> ownedBackgrounds = new List<int>();
    public List<int> ownedUpgrades = new List<int>();
    public int selectedAccessory = -1;
    public int selectedBackground = -1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadFromUser(User user)
    {
        if (user == null) return;

        currentUserId = user.id;
        score = user.score;
        clickLevel = user.click_level;
        passiveLevel = user.passive_level;

        LoadFromDB();
    }

    public void LoadFromDB()
    {
        var db = DatabaseManager.Instance.DB;
        var user = db.Table<User>().FirstOrDefault(x => x.id == currentUserId);
        if (user != null)
        {
            score = user.score;
        }

        var allItems = db.Table<UserItem>().Where(x => x.user_id == currentUserId).ToList();
        ownedAccessories = allItems.Where(x => x.item_type == "accessory").Select(x => x.item_id).ToList();
        ownedBackgrounds = allItems.Where(x => x.item_type == "background").Select(x => x.item_id).ToList();
        ownedUpgrades = allItems.Where(x => x.item_type == "upgrade").Select(x => x.item_id).ToList();

        selectedAccessory = allItems.FirstOrDefault(x => x.item_type == "accessory" && x.is_equipped == 1)?.item_id ?? -1;
        selectedBackground = allItems.FirstOrDefault(x => x.item_type == "background" && x.is_equipped == 1)?.item_id ?? -1;
    }
}