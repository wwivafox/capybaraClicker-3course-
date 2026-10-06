using System.IO;
using System;
using UnityEngine;
using SQLite4Unity3d;
using System.Linq;
using System.Collections.Generic;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }
    public SQLiteConnection DB { get; private set; }
    [SerializeField] private string dbName = "funny_capybara.db";

    private User _currentUser;
    public User CurrentUser
    {
        get => _currentUser;
        private set
        {
            _currentUser = value;
            if (_currentUser != null)
            {
                Debug.Log($"Текущий пользователь установлен: {_currentUser.username}");
            }
        }
    }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        var persistentPath = Path.Combine(Application.persistentDataPath, dbName);
        DB = new SQLiteConnection(persistentPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        DB.Execute("PRAGMA foreign_keys = ON");

        try
        {
            DB.CreateTable<User>(CreateFlags.AllImplicit);
            DB.CreateTable<Accessory>();
            DB.CreateTable<Background>();
            DB.CreateTable<Upgrade>();
            DB.CreateTable<UserItem>();

            //Debug.Log("Все таблицы успешно созданы");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка при создании таблиц: {ex.Message}");
            throw;
        }
    }


    public bool TryLoginUser(string username)
    {
        CurrentUser = DB.Table<User>().FirstOrDefault(u => u.username == username);

        if (CurrentUser == null)
        {
            CurrentUser = new User
            {
                username = username,
                score = 0,
                created_at = System.DateTime.UtcNow.ToString("o")
            };
            DB.Insert(CurrentUser);
            InitializeDefaultItems();
            return true;
        }

        UpdateLastLogin();
        return false; 
    }


    public void SetCurrentUser(User user)
    {
        if (user == null) return;

        _currentUser = DB.Table<User>().FirstOrDefault(u => u.id == user.id) ?? user;
        //Debug.Log($"Текущий пользователь установлен: {_currentUser.username}");
    }

    public void ClearCurrentUser()
    {
        _currentUser = null;
        //Debug.Log("Текущий пользователь сброшен");
    }

    private void InitializeDefaultItems()
    {
        
        AddUserItem("background", 0);
        EquipUserItem("background", 0);
    }

    private void UpdateLastLogin()
    {
        CurrentUser.last_active = System.DateTime.UtcNow.ToString("o");
        DB.Update(CurrentUser);
    }

    public void AddUserItem(string itemType, int itemId)
    {
        if (CurrentUser == null) return;

        if (!DB.Table<UserItem>().Any(x => x.user_id == CurrentUser.id &&
                                         x.item_type == itemType &&
                                         x.item_id == itemId))
        {
            DB.Insert(new UserItem
            {
                user_id = CurrentUser.id,
                item_type = itemType,
                item_id = itemId,
                is_equipped = 0
            });
        }
    }

   

    public void DeleteUserWithAllData(int userId)
    {
        try
        {
            DB.BeginTransaction();
            DB.Execute("DELETE FROM UserItem WHERE user_id = ?", userId);
            DB.Delete<User>(userId);
            DB.Commit();

            Debug.Log($"Пользователь {userId} и все связанные данные успешно удалены");

            if (CurrentUser?.id == userId)
            {
                ClearCurrentUser();
            }
        }
        catch (Exception ex)
        {
            DB.Rollback();
            Debug.LogError($"Ошибка при удалении пользователя {userId}: {ex.Message}");
            throw;
        }
    }

 

    public void EquipUserItem(string itemType, int itemId)
    {
        if (CurrentUser == null) return;

        DB.Execute("UPDATE user_items SET is_equipped = 0 WHERE user_id = ? AND item_type = ?",
                 CurrentUser.id, itemType);

        DB.Execute("UPDATE user_items SET is_equipped = 1 WHERE user_id = ? AND item_type = ? AND item_id = ?",
                 CurrentUser.id, itemType, itemId);
    }


    public List<int> GetUserItems(string itemType)
    {
        if (CurrentUser == null) return new List<int>();

        return DB.Table<UserItem>()
               .Where(x => x.user_id == CurrentUser.id && x.item_type == itemType)
               .Select(x => x.item_id)
               .ToList();
    }

    public int? GetEquippedItem(string itemType)
    {
        if (CurrentUser == null) return null;

        return DB.Table<UserItem>()
               .FirstOrDefault(x => x.user_id == CurrentUser.id &&
                                  x.item_type == itemType &&
                                  x.is_equipped == 1)?
               .item_id;
    }

    public void SaveUser(User user)
    {
        DB.Update(user);
    }
   
}