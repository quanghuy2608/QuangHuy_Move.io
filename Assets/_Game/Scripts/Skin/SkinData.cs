using UnityEngine;

[CreateAssetMenu(fileName = "SkinData", menuName = "Game/Skin Data")]
public class SkinData : ScriptableObject
{
    [System.Serializable]
    public class SkinItem
    {
        public string skinName;
        public Material pantMaterial;
        public Sprite skinIcon; 
        public int skinPrice = 0; 
        public bool isUnlocked = true; 
    }

    public SkinItem[] skinItems;

    public SkinItem GetSkin(int index)
    {
        if (index >= 0 && index < skinItems.Length)
        {
            return skinItems[index];
        }
        return null;
    }

    public int GetTotalSkins()
    {
        return skinItems.Length;
    }
}