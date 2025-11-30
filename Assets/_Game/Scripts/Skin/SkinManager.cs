
using UnityEngine;

public class SkinManager : Singleton<SkinManager>
{
    [Header("Skin Data")]
    public SkinData skinData;

    [Header("Player References")]
    public SkinnedMeshRenderer playerPantRenderer; 

    private int currentSkinIndex = 0;

    private void Start()
    {
        LoadSavedSkin();
    }

    public void ChangeSkin(int skinIndex)
    {
        if (skinData == null)
        {
            return;
        }

        SkinData.SkinItem skin = skinData.GetSkin(skinIndex);

        if (skin != null && skin.isUnlocked)
        {
            ApplySkin(skin);
            currentSkinIndex = skinIndex;
            SaveSkin(skinIndex);
        }
    }

    private void ApplySkin(SkinData.SkinItem skin)
    {
        if (playerPantRenderer != null && skin.pantMaterial != null)
        {
            playerPantRenderer.material = skin.pantMaterial;
        }
    }

    private void SaveSkin(int skinIndex)
    {
        PlayerPrefs.SetInt("CurrentSkin", skinIndex);
        PlayerPrefs.Save();
    }

    private void LoadSavedSkin()
    {
        currentSkinIndex = PlayerPrefs.GetInt("CurrentSkin", 0);
        ChangeSkin(currentSkinIndex);
    }

    public int GetCurrentSkinIndex()
    {
        return currentSkinIndex;
    }

    public void SetPlayerPantRenderer(SkinnedMeshRenderer renderer)
    {
        playerPantRenderer = renderer;
        LoadSavedSkin(); 
    }
}