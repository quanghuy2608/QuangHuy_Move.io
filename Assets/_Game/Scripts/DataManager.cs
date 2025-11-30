using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    private const string COIN_KEY = "PlayerCoin";
    private const string CURRENT_SKIN_KEY = "CurrentSkin";
    private const string CURRENT_WEAPON_KEY = "CurrentWeapon";
    private const string OWNED_SKINS_KEY = "OwnedSkins";
    private const string OWNED_WEAPONS_KEY = "OwnedWeapons";

    private int coin;
    private string currentSkin;
    private string currentWeapon;
    private List<string> ownedSkins = new List<string>();
    private List<string> ownedWeapons = new List<string>();

    public int Coin => coin;
    public string CurrentSkin => currentSkin;
    public string CurrentWeapon => currentWeapon;

    private void Awake()
    {
        LoadData();
    }

    public void LoadData()
    {
        coin = PlayerPrefs.GetInt(COIN_KEY, 0);
        currentSkin = PlayerPrefs.GetString(CURRENT_SKIN_KEY, "Default");
        currentWeapon = PlayerPrefs.GetString(CURRENT_WEAPON_KEY, "Axe");

        // Load owned skins
        string ownedSkinsData = PlayerPrefs.GetString(OWNED_SKINS_KEY, "Default");
        ownedSkins = new List<string>(ownedSkinsData.Split(','));

        // Load owned weapons
        string ownedWeaponsData = PlayerPrefs.GetString(OWNED_WEAPONS_KEY, "Axe");
        ownedWeapons = new List<string>(ownedWeaponsData.Split(','));

        Debug.Log($"Data loaded: Coin={coin}, Skin={currentSkin}, Weapon={currentWeapon}");
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt(COIN_KEY, coin);
        PlayerPrefs.SetString(CURRENT_SKIN_KEY, currentSkin);
        PlayerPrefs.SetString(CURRENT_WEAPON_KEY, currentWeapon);
        PlayerPrefs.SetString(OWNED_SKINS_KEY, string.Join(",", ownedSkins));
        PlayerPrefs.SetString(OWNED_WEAPONS_KEY, string.Join(",", ownedWeapons));
        PlayerPrefs.Save();

        //Debug.Log($"Data saved: Coin={coin}");
    }

    // Coin management
    public void AddCoin(int amount)
    {
        coin += amount;
        SaveData();

        // TODO: Update UI
        //Debug.Log($"Added {amount} coins. Total: {coin}");
    }

    public bool SpendCoin(int amount)
    {
        if (coin >= amount)
        {
            coin -= amount;
            SaveData();
            
            return true;
        }

        //Debug.Log($"Not enough coins! Have {coin}, need {amount}");
        return false;
    }

    // Skin management
    public void UnlockSkin(string skinID)
    {
        if (ownedSkins.Contains(skinID))
        {
            Debug.Log($"Already owned skin: {skinID}");
            return;
        }

        ownedSkins.Add(skinID);
        SaveData();
        Debug.Log($"Unlocked skin: {skinID}");
    }

    public void UnlockSkin(string skinName, int price)
    {
        if (ownedSkins.Contains(skinName))
        {
            Debug.Log($"Already owned skin: {skinName}");
            return;
        }

        if (SpendCoin(price))
        {
            ownedSkins.Add(skinName);
            SaveData();
            Debug.Log($"Unlocked skin: {skinName}");
        }
    }

    public void SelectSkin(string skinName)
    {
        if (ownedSkins.Contains(skinName))
        {
            currentSkin = skinName;
            SaveData();
            Debug.Log($"Selected skin: {skinName}");

            // TODO: Apply skin to player model
        }
    }

    public bool IsSkinOwned(string skinName)
    {
        return ownedSkins.Contains(skinName);
    }

    // Weapon management
    public void UnlockWeapon(string weaponName, int price)
    {
        if (ownedWeapons.Contains(weaponName))
        {
            Debug.Log($"Already owned weapon: {weaponName}");
            return;
        }

        if (SpendCoin(price))
        {
            ownedWeapons.Add(weaponName);
            SaveData();
            Debug.Log($"Unlocked weapon: {weaponName}");
        }
    }

    public void SelectWeapon(string weaponName)
    {
        if (ownedWeapons.Contains(weaponName))
        {
            currentWeapon = weaponName;
            SaveData();
            Debug.Log($"Selected weapon: {weaponName}");

            // TODO: Apply weapon to player
        }
    }

    public bool IsWeaponOwned(string weaponName)
    {
        return ownedWeapons.Contains(weaponName);
    }

    // Reset data (for testing)
    public void ResetData()
    {
        PlayerPrefs.DeleteAll();
        LoadData();
        Debug.Log("Data reset!");
    }
}