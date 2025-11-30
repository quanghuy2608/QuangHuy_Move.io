using UnityEngine;
using UnityEngine.UI;

public class SkinSelectionUI : UICanvas
{
    [Header("Skin Buttons")]
    public Button[] skinButtons; 

    [Header("Back Button")]
    public Button backButton;

    [Header("Preview")]
    public GameObject playerPreviewPrefab; 
    public Transform previewSpawnPoint; 
    public float rotationSpeed = 50f; 

    [Header("Preview Alternative - Use Real Player")]
    public bool useRealPlayer = false; 
    public Transform realPlayerTransform; 
    public Vector3 previewPosition = new Vector3(0, 0, 5); 
    public Vector3 originalPosition; 

    private GameObject currentPreviewPlayer;
    private SkinnedMeshRenderer previewPantRenderer;
    private bool isShowingPreview = false;

    private void Awake()
    {
        SetupButtons();
    }

    public override void Setup()
    {
        base.Setup();
        SpawnPreviewPlayer();
        UpdateButtonStates();
    }

    private void SetupButtons()
    {

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        for (int i = 0; i < skinButtons.Length; i++)
        {
            int skinIndex = i; 
            if (skinButtons[i] != null)
            {
                skinButtons[i].onClick.AddListener(() => OnSkinButtonClicked(skinIndex));
            }
        }
    }

    private void SpawnPreviewPlayer()
    {
        if (useRealPlayer && realPlayerTransform != null)
        {
            currentPreviewPlayer = realPlayerTransform.gameObject;

            originalPosition = realPlayerTransform.position;

            realPlayerTransform.position = previewPosition;
            previewPantRenderer = currentPreviewPlayer.GetComponentInChildren<SkinnedMeshRenderer>();


            DisablePlayerComponents();

            isShowingPreview = true;
        }
        else if (playerPreviewPrefab != null && previewSpawnPoint != null)
        {

            if (currentPreviewPlayer != null)
            {
                Destroy(currentPreviewPlayer);
            }

            currentPreviewPlayer = Instantiate(playerPreviewPrefab, previewSpawnPoint.position, previewSpawnPoint.rotation);
            currentPreviewPlayer.transform.SetParent(previewSpawnPoint);

            previewPantRenderer = currentPreviewPlayer.GetComponentInChildren<SkinnedMeshRenderer>();

            isShowingPreview = true;
        }

        if (isShowingPreview)
        {
            int currentSkin = SkinManager.Instance.GetCurrentSkinIndex();
            ApplyPreviewSkin(currentSkin);
        }
    }

    private void DisablePlayerComponents()
    {
        if (currentPreviewPlayer == null) return;

        var rb = currentPreviewPlayer.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        var colliders = currentPreviewPlayer.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

    }

    private void EnablePlayerComponents()
    {
        if (currentPreviewPlayer == null || !useRealPlayer) return;

        var rb = currentPreviewPlayer.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        var colliders = currentPreviewPlayer.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = true;
        }
    }

    private void Update()
    {

        if (currentPreviewPlayer != null)
        {
            currentPreviewPlayer.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    private void OnSkinButtonClicked(int skinIndex)
    {

        SkinManager.Instance.ChangeSkin(skinIndex);

        ApplyPreviewSkin(skinIndex);

        UpdateButtonStates();
    }

    private void ApplyPreviewSkin(int skinIndex)
    {
        if (previewPantRenderer != null && SkinManager.Instance.skinData != null)
        {
            SkinData.SkinItem skin = SkinManager.Instance.skinData.GetSkin(skinIndex);
            if (skin != null && skin.pantMaterial != null)
            {
                previewPantRenderer.material = skin.pantMaterial;
            }
        }
    }

    private void UpdateButtonStates()
    {
        int currentSkin = SkinManager.Instance.GetCurrentSkinIndex();

        for (int i = 0; i < skinButtons.Length; i++)
        {
            if (skinButtons[i] != null)
            {
                Transform selected = skinButtons[i].transform.Find("Selected");
                if (selected != null)
                {
                    selected.gameObject.SetActive(i == currentSkin);
                }
            }
        }
    }

    private void OnBackButtonClicked()
    {
        UIManager.Instance.OpenUI<MainMenu>();
        Close();
    }

    public override void BackKey()
    {
        OnBackButtonClicked();
    }

    public override void CloseDirectly()
    {
        if (useRealPlayer && realPlayerTransform != null && isShowingPreview)
        {
            realPlayerTransform.position = originalPosition;
            EnablePlayerComponents();
        }

        if (!useRealPlayer && currentPreviewPlayer != null)
        {
            Destroy(currentPreviewPlayer);
        }

        isShowingPreview = false;
        base.CloseDirectly();
    }
}