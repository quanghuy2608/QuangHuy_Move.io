using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class LevelManager : Singleton<LevelManager>
{
    [Header("Level Data")]
    [SerializeField] private Level[] levelPrefabs;
    public Player player;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int maxTotalBots = 10;
    [SerializeField] private int botsPerWave = 5;

    [Header("Camera Settings")]
    [SerializeField] private CinemachineVirtualCamera menuCam;
    [SerializeField] private CinemachineVirtualCamera gameplayCam;
    [SerializeField] private CinemachineVirtualCamera skinSelectCam;
    [SerializeField] private float baseCameraDistance = 15f;
    [SerializeField] private float cameraZoomPerSize = 2f;

    [Header("Delays")]
    [SerializeField] private float failUIDelay = 2f;
    [SerializeField] private float victoryUIDelay = 1f;

    // Private state
    private Level currentLevel;
    private List<Bot> activeBots = new List<Bot>();
    private int levelIndex = 0;
    private int totalBotsSpawned = 0;
    private bool isGamePlaying = false;

    private int AliveBotsCount => activeBots.Count;

    #region Initialization
    private void Awake()
    {
        levelIndex = PlayerPrefs.GetInt("Level", 0);
    }

    private void Start()
    {
        LoadLevel(levelIndex);
        InitializeLevel();
        SwitchCamera(CameraMode.Menu);

        // Play menu music
        AudioManager.Instance.PlayMusic(SoundType.MUSIC_MENU);

        UIManager.Instance.OpenUI<MainMenu>();
    }

    public void InitializeLevel()
    {
        ResetGameState();
        SpawnAllCharacters();
    }

    private void ResetGameState()
    {
        totalBotsSpawned = 0;
        isGamePlaying = false;
        activeBots.Clear();
    }
    #endregion

    #region Level Management
    public void LoadLevel(int level)
    {
        // Cleanup old level
        if (currentLevel != null)
        {
            Destroy(currentLevel.gameObject);
        }

        // Validate level index
        if (!IsValidLevelIndex(level))
        {
            Debug.LogError($"Invalid level index: {level}");
            return;
        }

        // Instantiate new level
        currentLevel = Instantiate(levelPrefabs[level]);
        currentLevel.OnInit();
    }

    private bool IsValidLevelIndex(int level)
    {
        return levelPrefabs != null &&
               levelPrefabs.Length > 0 &&
               level >= 0 &&
               level < levelPrefabs.Length &&
               levelPrefabs[level] != null;
    }
    #endregion

    #region Character Spawning
    private void SpawnAllCharacters()
    {
        if (!ValidateSpawnPoints())
            return;

        List<Transform> shuffledPoints = GetShuffledSpawnPoints();

        // Spawn player at first point
        SpawnPlayer(shuffledPoints[0]);

        // Spawn initial wave of bots
        for (int i = 0; i < botsPerWave && i < maxTotalBots; i++)
        {
            SpawnBot(shuffledPoints[i + 1].position, startMoving: false);
        }
    }

    private bool ValidateSpawnPoints()
    {
        if (spawnPoints == null || spawnPoints.Length < botsPerWave + 1)
        {
            Debug.LogError($"Not enough spawn points! Need {botsPerWave + 1}, have {spawnPoints?.Length ?? 0}");
            return false;
        }
        return true;
    }

    private List<Transform> GetShuffledSpawnPoints()
    {
        List<Transform> shuffled = new List<Transform>(spawnPoints);

        // Fisher-Yates shuffle
        for (int i = 0; i < shuffled.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffled.Count);
            Transform temp = shuffled[i];
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        return shuffled;
    }

    private void SpawnPlayer(Transform spawnPoint)
    {
        player.transform.position = spawnPoint.position;
        player.transform.rotation = Quaternion.Euler(0, 180, 0);
        player.OnInit();
    }

    private void SpawnBot(Vector3 position, bool startMoving)
    {
        if (totalBotsSpawned >= maxTotalBots)
            return;

        // Find clear spawn position
        Vector3 spawnPos = FindClearSpawnPosition(position);

        // Spawn bot
        Bot bot = SimplePool.Spawn<Bot>(PoolType.Bot, spawnPos, Quaternion.identity);
        bot.OnInit();

        // Set initial state
        if (startMoving)
        {
            bot.ChangeState(new PatrolState());
        }
        else
        {
            bot.ChangeAnim("idle");
        }

        // Track bot
        activeBots.Add(bot);
        totalBotsSpawned++;
    }

    private Vector3 FindClearSpawnPosition(Vector3 preferredPosition)
    {
        // Check if preferred position is clear
        if (IsPositionClear(preferredPosition))
            return preferredPosition;

        // Try to find alternative position nearby
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * 5f;
            Vector3 testPosition = preferredPosition + new Vector3(randomOffset.x, 0, randomOffset.y);

            if (UnityEngine.AI.NavMesh.SamplePosition(testPosition, out UnityEngine.AI.NavMeshHit hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                if (IsPositionClear(hit.position))
                    return hit.position;
            }
        }

        // Fallback to preferred position
        return preferredPosition;
    }

    private bool IsPositionClear(Vector3 position, float radius = 1f)
    {
        Collider[] overlaps = Physics.OverlapSphere(position, radius);
        foreach (Collider col in overlaps)
        {
            if (col.GetComponent<Character>() != null)
                return false;
        }
        return true;
    }
    #endregion

    #region Game Flow
    public void OnStartGame()
    {
        isGamePlaying = true;

        GameManager.Instance.ChangeState(GameState.GamePlay);
        UIManager.Instance.OpenUI<GamePlay>();
        UIManager.Instance.CloseUI<MainMenu>();

        SwitchCamera(CameraMode.Gameplay);

        // Play gameplay music
        AudioManager.Instance.PlayMusic(SoundType.MUSIC_GAMEPLAY, fadeIn: true, fadeDuration: 1f);

        // Start all bots patrolling
        foreach (Bot bot in activeBots)
        {
            if (bot != null && bot.gameObject.activeSelf)
            {
                bot.ChangeState(new PatrolState());
            }
        }
    }

    public void OnFinishGame()
    {
        isGamePlaying = false;
        StopAllBots();
    }

    public void OnPlayerDeath()
    {
        if (!isGamePlaying)
            return;

        isGamePlaying = false;
        StopAllBots();
        AudioManager.Instance.PlaySFX(SoundType.FAIL);

        StartCoroutine(ShowUIAfterDelay<Fail>(failUIDelay));
    }

    public void OnPlayerWin()
    {
        if (!isGamePlaying)
            return;

        isGamePlaying = false;
        player.ChangeAnim("win");
        DataManager.Instance.AddCoin(5);
        AudioManager.Instance.PlaySFX(SoundType.VICTORY);

        StartCoroutine(ShowUIAfterDelay<Victory>(victoryUIDelay));
    }

    public void OnBotDeath(Bot deadBot)
    {
        activeBots.Remove(deadBot);
        if (totalBotsSpawned < maxTotalBots)
        {
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            SpawnBot(randomPoint.position, startMoving: true);
        }
        CheckWinCondition();
    }

    private void StopAllBots()
    {
        foreach (Bot bot in activeBots)
        {
            if (bot != null && bot.gameObject.activeSelf)
            {
                bot.ChangeState(null);
                bot.MoveStop();
                bot.ChangeAnim("idle");
            }
        }
    }

    private void CheckWinCondition()
    {
        if (!isGamePlaying)
            return;
        if (AliveBotsCount == 0 && totalBotsSpawned >= maxTotalBots)
        {
            OnPlayerWin();
        }
    }
    #endregion

    #region Camera Management
    private enum CameraMode { Menu, Gameplay, SkinSelect }

    public void ShowMenuCamera() => SwitchCamera(CameraMode.Menu);
    public void ShowGameplayCamera() => SwitchCamera(CameraMode.Gameplay);
    public void ShowSkinSelectCamera() => SwitchCamera(CameraMode.SkinSelect);

    private void SwitchCamera(CameraMode mode)
    {
        if (menuCam == null || gameplayCam == null)
            return;

        menuCam.Priority = mode == CameraMode.Menu ? 10 : 0;
        gameplayCam.Priority = mode == CameraMode.Gameplay ? 10 : 0;

        if (skinSelectCam != null)
        {
            skinSelectCam.Priority = mode == CameraMode.SkinSelect ? 10 : 0;
        }
    }

    public void OnCharacterGrow(float newSize)
    {
        if (gameplayCam == null)
            return;

        float targetDistance = baseCameraDistance + ((newSize - 1f) * cameraZoomPerSize);

        var transposer = gameplayCam.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            transposer.m_FollowOffset = new Vector3(0, targetDistance, -targetDistance);
        }
    }
    #endregion

    #region Reset & Restart
    public void OnReset()
    {
        SimplePool.CollectAll();
        activeBots.Clear();
        ResetGameState();
        UIManager.Instance.CloseAll();
    }

    public void OnRetry()
    {
        OnReset();
        LoadLevel(levelIndex);
        InitializeLevel();
        ShowGameplayCamera();
        OnStartGame();
    }

    public void OnNextLevel()
    {
        levelIndex++;
        PlayerPrefs.SetInt("Level", levelIndex);

        OnReset();
        LoadLevel(levelIndex);
        InitializeLevel();
        ShowMenuCamera();
        UIManager.Instance.OpenUI<MainMenu>();
    }
    #endregion

    #region Utilities
    private IEnumerator ShowUIAfterDelay<T>(float delay) where T : UICanvas
    {
        yield return new WaitForSeconds(delay);
        UIManager.Instance.CloseAll();
        UIManager.Instance.OpenUI<T>();
    }
    #endregion
}