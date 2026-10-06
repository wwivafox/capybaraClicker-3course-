using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SQLite4Unity3d;

public class ProfileMenu : MonoBehaviour
{
    public TMP_InputField profileInputField;
    public Button createButton, deleteButton, selectButton, exitToMenuButton;
    public Transform profilesContent;
    public GameObject profileItemPrefab;

    private List<User> profiles = new List<User>();
    private User selectedProfile = null;
    private GameObject selectedProfileItem = null;

    void Start()
    {
        LoadProfiles();

        createButton.onClick.AddListener(CreateProfile);
        deleteButton.onClick.AddListener(DeleteProfile); 
        selectButton.onClick.AddListener(SelectProfileConfirm);
        exitToMenuButton.onClick.AddListener(ExitToMenu);
        UpdateButtonsState();
    }

    private void LoadProfiles()
    {
        profiles = DatabaseManager.Instance.DB.Table<User>().ToList();
        RefreshProfileList();
    }

    private void RefreshProfileList()
    {
        foreach (Transform child in profilesContent)
            Destroy(child.gameObject);

        foreach (var profile in profiles)
        {
            var item = Instantiate(profileItemPrefab, profilesContent);
            var itemText = item.GetComponentInChildren<TMP_Text>();
            itemText.text = profile.username;

            var button = item.GetComponent<Button>();
            var capturedProfile = profile;
            button.onClick.AddListener(() => {
                SelectProfile(capturedProfile);
                HighlightSelectedProfile(item);
            });

            if (selectedProfile != null && profile.id == selectedProfile.id)
                HighlightSelectedProfile(item);
            else
                item.GetComponent<Image>().color = Color.white;
        }
    }

    private void HighlightSelectedProfile(GameObject item)
    {
        if (selectedProfileItem != null && selectedProfileItem.GetComponent<Image>())
            selectedProfileItem.GetComponent<Image>().color = Color.white;

        selectedProfileItem = item;
        var img = selectedProfileItem.GetComponent<Image>();
        if (img != null)
            img.color = new Color(1f, 0.8f, 0f, 1f);
    }

    public void SelectProfile(User profile)
    {
        var loadedUser = DatabaseManager.Instance.DB.Table<User>().FirstOrDefault(u => u.id == profile.id);

        if (loadedUser != null)
        {
            selectedProfile = loadedUser;
            DatabaseManager.Instance.SetCurrentUser(loadedUser);

            PlayerPrefs.SetInt("currentUserId", loadedUser.id);

            UpdateButtonsState();

            if (selectedProfileItem != null)
                HighlightSelectedProfile(selectedProfileItem);

            ShowProfileInfo(loadedUser);
        }
        else
        {
            selectedProfile = profile;
        }
    }

    public void SelectProfileConfirm()
    {
        if (selectedProfile == null) return;

        if (selectedProfileItem != null)
            StartCoroutine(FlashSelection(selectedProfileItem.GetComponent<Image>()));

        NotifyProfileSelected(true);
        PlayerPrefs.SetInt("currentUserId", selectedProfile.id);
        ExitToMenu();
    }

    public void CreateProfile()
    {
        if (string.IsNullOrEmpty(profileInputField.text))
        {
            Debug.LogWarning("Имя профиля не может быть пустым");
            return;
        }

        try
        {
            var db = DatabaseManager.Instance.DB;

            if (db.Table<User>().Any(u => u.username == profileInputField.text))
            {
                Debug.LogWarning("Профиль с таким именем уже существует");
                return;
            }

            var newUser = new User
            {
                username = profileInputField.text,
                score = 0,
                created_at = DateTime.UtcNow.ToString("o")
            };

            db.Insert(newUser);
            Debug.Log($"Создан новый профиль: {newUser.username} (ID: {newUser.id})");

            LoadProfiles();
        }
        catch (SQLiteException ex)
        {
            Debug.LogError($"Ошибка при создании профиля: {ex.Message}");

        }
    }
    public void DeleteProfile()
    {
        if (selectedProfile == null) return;

        try
        {
           
            int userId = selectedProfile.id;

          
            DatabaseManager.Instance.DeleteUserWithAllData(userId);

            Debug.Log($"Профиль {selectedProfile.username} (ID: {userId}) и все связанные данные удалены");

            NotifyProfileSelected(false);
            selectedProfile = null;
            selectedProfileItem = null;
            LoadProfiles();
            UpdateButtonsState();

            if (PlayerPrefs.GetInt("currentUserId") == userId)
            {
                PlayerPrefs.DeleteKey("currentUserId");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка при удалении профиля: {ex.Message}");
        }
    }


    public void ExitToMenu()
    {
        var menuManager = FindObjectOfType<MainMenuManager>();
        if (menuManager != null)
            menuManager.BackToMenu();
    }

    private void UpdateButtonsState()
    {
        bool selected = selectedProfile != null;
        deleteButton.interactable = selected;
        selectButton.interactable = selected;
    }

    private IEnumerator FlashSelection(Image image)
    {
        if (image == null) yield break;
        Color originalColor = image.color;
        image.color = new Color(1f, 0.6f, 0f, 1f);
        yield return new WaitForSeconds(0.2f);
        image.color = originalColor;
    }

    private void ShowProfileInfo(User user)
    {
        Debug.Log($"Профиль: {user.username}, Score: {user.score}, " +
                 $"Click Level: {user.click_level}, Passive Level: {user.passive_level}");
    }

    private void NotifyProfileSelected(bool isSelected)
    {
        var menuManager = FindObjectOfType<MainMenuManager>();
        if (menuManager != null)
        {
            if (isSelected)
                menuManager.OnProfileSelected();
            else
                menuManager.OnProfileDeselected();
        }
    }
}