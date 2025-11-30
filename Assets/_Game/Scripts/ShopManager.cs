using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShopItem
{
    public string itemName;
    public int price;
    public ItemType type;
    public GameObject prefab; // Model skin hoac weapon
}

public enum ItemType
{
    Skin,
    Weapon
}

public class ShopManager : MonoBehaviour
{
    [Header("Shop Items")]
    [SerializeField] private List<ShopItem> shopItems = new List<ShopItem>();

    [Header("Preview")]
    [SerializeField] private Transform previewHolder; // Vi tri hien thi skin/weapon preview
    private GameObject currentPreview;

    private void Start()
    {
        // Load trang bi hien tai
        LoadCurrentEquipment();
    }

    public void BuyItem(string itemName)
    {
        ShopItem item = shopItems.Find(x => x.itemName == itemName);

        if (item == null)
        {
            Debug.LogError($"Item not found: {itemName}");
            return;
        }

        // Kiem tra da mua chua
        if (item.type == ItemType.Skin)
        {
            if (DataManager.Instance.IsSkinOwned(itemName))
            {
                Debug.Log($"Already owned: {itemName}");
                return;
            }

            DataManager.Instance.UnlockSkin(itemName, item.price);
        }
        else if (item.type == ItemType.Weapon)
        {
            if (DataManager.Instance.IsWeaponOwned(itemName))
            {
                Debug.Log($"Already owned: {itemName}");
                return;
            }

            DataManager.Instance.UnlockWeapon(itemName, item.price);
        }

        // TODO: Update UI
    }

    public void SelectItem(string itemName, ItemType type)
    {
        if (type == ItemType.Skin)
        {
            DataManager.Instance.SelectSkin(itemName);
            PreviewSkin(itemName);
        }
        else if (type == ItemType.Weapon)
        {
            DataManager.Instance.SelectWeapon(itemName);
            PreviewWeapon(itemName);
        }
    }

    private void PreviewSkin(string skinName)
    {
        // Xoa preview cu
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }

        // Tao preview moi
        ShopItem item = shopItems.Find(x => x.itemName == skinName && x.type == ItemType.Skin);
        if (item != null && item.prefab != null)
        {
            currentPreview = Instantiate(item.prefab, previewHolder);
            currentPreview.transform.localPosition = Vector3.zero;
            currentPreview.transform.localRotation = Quaternion.identity;

            Debug.Log($"Previewing skin: {skinName}");
        }
    }

    private void PreviewWeapon(string weaponName)
    {
        // TODO: Hien thi weapon tren tay player trong preview
       //Debug.Log($"Previewing weapon: {weaponName}");
    }

    private void LoadCurrentEquipment()
    {
        // Load skin va weapon hien tai khi vao shop
        string currentSkin = DataManager.Instance.CurrentSkin;
        string currentWeapon = DataManager.Instance.CurrentWeapon;

        PreviewSkin(currentSkin);
        PreviewWeapon(currentWeapon);
    }

    public List<ShopItem> GetAllItems()
    {
        return shopItems;
    }

    public List<ShopItem> GetItemsByType(ItemType type)
    {
        return shopItems.FindAll(x => x.type == type);
    }
}