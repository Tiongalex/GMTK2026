using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public enum GameState { Playing, MiniGame, Won, Lost }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Configuration")]
    [SerializeField] private float totalTime = 100f;
    [SerializeField] private int requiredSealedObjects = 3;
    [SerializeField] private List<CursedObject> cursedObjectPool;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject escapeDoor;
    [SerializeField] private float spawnDelay = 3.0f; // Delay before activating the next object

    [Header("Ghost Object Settings")]
    [SerializeField] private string ghostTag = "Ghost";

    [Header("Scene Navigation")]
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    [Header("Current Status")]
    public float TimeRemaining { get; private set; }
    public int SealedObjectsCount { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.Playing;
    public CursedObject ActiveCursedObject { get; private set; }

    public event Action<GameState> OnStateChanged;
    public event Action<CursedObject> OnNewObjectSpawned;
    public event Action<int> OnObjectSealed;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioSource tickingAudioSource;

    [Header("Ticking Sound Configuration")]
    [SerializeField] private float minPitch = 1.0f;
    [SerializeField] private float maxPitch = 1.0f;
    [SerializeField] private float maxVolume = 2.0f;

    [Header("Lighting Settings")]
    [SerializeField] private List<Light> roomLights = new List<Light>();
    [SerializeField] private Color normalColor = new Color(212f / 255f, 100f / 255f, 40f / 255f, 1f);
    [SerializeField] private Color corruptedColor = new Color(0.396f, 0f, 0f, 1f);
    [SerializeField] private float minIntensityMultiplier = 0.4f;
    [SerializeField] private bool enableFlicker = true;

    [Header("Flicker Tuning")]
    [SerializeField] private float flickerSpeed = 9f;
    [SerializeField] private float maxFlickerSeverity = 0.9f;

    private Dictionary<Light, float> originalIntensities = new Dictionary<Light, float>();

    [Header("Heartbeat Settings")]
    [SerializeField] private AudioSource heartbeatAudioSource;
    [SerializeField] private float heartbeatTriggerTime = 30f;

    private bool heartbeatStarted = false;
    private bool deathSequenceTriggered = false;
    private Coroutine spawnCoroutine;

    [Header("Zero-Second Sequence Audio")]
    [SerializeField] private AudioSource doorOpenAudioSource;
    [SerializeField] private AudioSource jumpscareAudioSource;

    [Header("Candle Death Audio")]
    [SerializeField] private AudioSource crazyDeathAudioSource;

    [Header("Custom UI References")]
    [SerializeField] private CanvasGroup fadeScreenCanvasGroup; // The CanvasGroup on EndGamePanel
    [SerializeField] private TextMeshProUGUI endMessageText;    // Text child inside EndGamePanel
    [SerializeField] private Button playAgainButton;           // Play Again Button inside EndGamePanel
    [SerializeField] private Button exitMenuButton;            // Exit To Menu Button inside EndGamePanel
    [SerializeField] private float fadeDuration = 0.8f;

    [Header("Player Control & Rigidbody Freeze")]
    [SerializeField] private MonoBehaviour playerControllerScript; // Drag your player movement/camera script here
    [SerializeField] private MonoBehaviour playerLookScript;       // Drag your mouse look script here (if separate)
    [SerializeField] private Rigidbody playerRigidbody;           // Drag player's Rigidbody here to freeze physics

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        TimeRemaining = totalTime;
        SealedObjectsCount = 0;

        if (escapeDoor == null)
        {
            EscapeDoor door = FindObjectOfType<EscapeDoor>();
            if (door != null) escapeDoor = door.gameObject;
        }

        if (cursedObjectPool == null || cursedObjectPool.Count == 0)
        {
            cursedObjectPool = new List<CursedObject>(FindObjectsOfType<CursedObject>());
        }

        // Make sure all objects start deactivated by default
        foreach (CursedObject obj in cursedObjectPool)
        {
            if (obj != null) obj.DeactivateObject();
        }

        if (cursedObjectPool.Count > 0)
        {
            requiredSealedObjects = Mathf.Min(requiredSealedObjects, cursedObjectPool.Count);
        }

        // Hide UI on start
        if (fadeScreenCanvasGroup != null)
        {
            fadeScreenCanvasGroup.alpha = 0f;
            fadeScreenCanvasGroup.blocksRaycasts = false;
            fadeScreenCanvasGroup.interactable = false;
        }

        InitializeLighting();

        // Schedule the first activation after the 3-second delay
        ScheduleNextCursedObject();

        if (backgroundMusicSource != null && !backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.loop = true;
            backgroundMusicSource.Play();
        }

        if (tickingAudioSource != null)
        {
            tickingAudioSource.loop = true;
            tickingAudioSource.volume = 0.5f;
            tickingAudioSource.pitch = minPitch;
            tickingAudioSource.Play();
        }
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing && CurrentState != GameState.MiniGame) return;

        TimeRemaining -= Time.deltaTime;

        UpdateTickingAudio();
        UpdateRoomLighting();

        // Condition 1: Candle light reached 0% -> Insanity Death
        if (CandleController.Instance != null && CandleController.Instance.CandlePower <= 0f && !deathSequenceTriggered)
        {
            deathSequenceTriggered = true;
            DestroyAllGhostObjects();
            TriggerLoss("Candle burned out!");
            StartCoroutine(CrazyDeathSequenceCoroutine());
            return;
        }

        // Condition 2: Time ran out -> Time Out Death
        if (TimeRemaining <= 0 && !deathSequenceTriggered)
        {
            deathSequenceTriggered = true;
            TimeRemaining = 0;
            DestroyAllGhostObjects();
            TriggerLoss("Time's Up");
            StartCoroutine(DeathSequenceCoroutine());
        }

        if (TimeRemaining <= heartbeatTriggerTime && !heartbeatStarted)
        {
            heartbeatStarted = true;
            if (heartbeatAudioSource != null)
            {
                heartbeatAudioSource.time = 0f;
                heartbeatAudioSource.Play();
            }
        }
    }

    private void FreezePlayer()
    {
        if (playerControllerScript != null) playerControllerScript.enabled = false;
        if (playerLookScript != null) playerLookScript.enabled = false;

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.isKinematic = true;
        }
    }

    private void DestroyAllGhostObjects()
    {
        if (!string.IsNullOrEmpty(ghostTag))
        {
            GameObject[] ghostsByTag = GameObject.FindGameObjectsWithTag(ghostTag);
            foreach (GameObject ghost in ghostsByTag)
            {
                if (ghost != null) Destroy(ghost);
            }
        }

        GhostAI[] ghostScripts = FindObjectsOfType<GhostAI>();
        foreach (GhostAI ghost in ghostScripts)
        {
            if (ghost != null) Destroy(ghost.gameObject);
        }
    }

    private IEnumerator CrazyDeathSequenceCoroutine()
    {
        FreezePlayer();
        if (crazyDeathAudioSource != null) crazyDeathAudioSource.Play();

        yield return StartCoroutine(FadeAndShowEndUI("Darkness consumed your mind.\nYou fell into total insanity."));
    }

    private IEnumerator DeathSequenceCoroutine()
    {
        FreezePlayer();

        if (doorOpenAudioSource != null) doorOpenAudioSource.Play();
        yield return new WaitForSeconds(2.0f);

        if (jumpscareAudioSource != null) jumpscareAudioSource.Play();

        yield return StartCoroutine(FadeAndShowEndUI("Time ran out.\nThe entity dragged you into the shadows."));
    }

    public void TriggerWin()
    {
        SetState(GameState.Won);
        StopAllGameplayAudio();
        DestroyAllGhostObjects();

        if (escapeDoor != null) escapeDoor.SetActive(false);

        StartCoroutine(WinSequenceCoroutine());
    }

    private IEnumerator WinSequenceCoroutine()
    {
        FreezePlayer();

        yield return StartCoroutine(FadeAndShowEndUI("Congratulations!\nYou broke the curse and escaped the room."));
    }

    private IEnumerator FadeAndShowEndUI(string resultMessage)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (endMessageText != null)
        {
            endMessageText.text = resultMessage;
        }

        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
            playAgainButton.onClick.AddListener(RestartGame);
        }

        if (exitMenuButton != null)
        {
            exitMenuButton.onClick.RemoveAllListeners();
            exitMenuButton.onClick.AddListener(ExitToMenu);
        }

        if (fadeScreenCanvasGroup != null)
        {
            fadeScreenCanvasGroup.gameObject.SetActive(true);
            fadeScreenCanvasGroup.blocksRaycasts = true;
            fadeScreenCanvasGroup.interactable = true;

            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                fadeScreenCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }
            fadeScreenCanvasGroup.alpha = 1f;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("[GameManager] Main menu scene name is not assigned!");
        }
    }

    private void UpdateTickingAudio()
    {
        if (tickingAudioSource == null || totalTime <= 0f) return;

        float elapsedRatio = Mathf.Clamp01((totalTime - TimeRemaining) / totalTime);

        if (heartbeatAudioSource != null)
        {
            heartbeatAudioSource.volume = elapsedRatio * maxVolume;
        }

        float pitchProgress = Mathf.InverseLerp(0.8f, 1.0f, elapsedRatio);
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.pitch = Mathf.Lerp(minPitch, maxPitch, pitchProgress);
        }
    }

    #region Lighting Logic

    private void InitializeLighting()
    {
        if (roomLights.Count == 0)
        {
            GameObject[] taggedObjs = GameObject.FindGameObjectsWithTag("HorrorLight");
            foreach (GameObject obj in taggedObjs)
            {
                if (obj.TryGetComponent<Light>(out Light l))
                {
                    roomLights.Add(l);
                }
            }
        }

        foreach (Light light in roomLights)
        {
            if (light != null)
            {
                originalIntensities[light] = light.intensity;
            }
        }
    }

    private void UpdateRoomLighting()
    {
        if (roomLights.Count == 0 || totalTime <= 0f) return;

        float elapsedRatio = Mathf.Clamp01((totalTime - TimeRemaining) / totalTime);
        Color currentColor = Color.Lerp(normalColor, corruptedColor, elapsedRatio);
        float intensityFactor = Mathf.Lerp(1.0f, minIntensityMultiplier, elapsedRatio);

        float flickerModifier = 1.0f;
        if (enableFlicker)
        {
            float rawNoise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
            float currentSeverity = elapsedRatio * maxFlickerSeverity;

            if (rawNoise > (1.0f - currentSeverity))
            {
                flickerModifier = Mathf.Lerp(1.0f, 1.0f - currentSeverity, rawNoise);
            }
        }

        foreach (Light light in roomLights)
        {
            if (light == null) continue;

            light.color = currentColor;
            float baseIntensity = originalIntensities.ContainsKey(light) ? originalIntensities[light] : 1f;
            light.intensity = baseIntensity * intensityFactor * flickerModifier;
        }
    }

    #endregion

    private EscapeDoor GetEscapeDoorComponent()
    {
        if (escapeDoor == null) return FindObjectOfType<EscapeDoor>();
        EscapeDoor door = escapeDoor.GetComponent<EscapeDoor>();
        if (door == null) door = escapeDoor.GetComponentInChildren<EscapeDoor>();
        if (door == null) door = escapeDoor.GetComponentInParent<EscapeDoor>();
        if (door == null) door = FindObjectOfType<EscapeDoor>();
        return door;
    }

    public void ScheduleNextCursedObject()
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnCursedObjectWithDelay());
    }

    private IEnumerator SpawnCursedObjectWithDelay()
    {
        // Ensure current active object is safely turned off
        if (ActiveCursedObject != null)
        {
            ActiveCursedObject.DeactivateObject();
            ActiveCursedObject = null;
        }

        bool allSealed = SealedObjectsCount >= requiredSealedObjects;
        if (!allSealed && cursedObjectPool != null && cursedObjectPool.Count > 0)
        {
            allSealed = cursedObjectPool.FindAll(o => !o.IsSealed).Count == 0;
        }

        if (allSealed)
        {
            EscapeDoor doorComponent = GetEscapeDoorComponent();
            if (doorComponent != null) doorComponent.Unlock();
            yield break;
        }

        // Wait 3 seconds before choosing and activating the next cursed object
        yield return new WaitForSeconds(spawnDelay);

        List<CursedObject> available = cursedObjectPool.FindAll(o => !o.IsSealed);
        if (available.Count == 0) yield break;

        ActiveCursedObject = available[UnityEngine.Random.Range(0, available.Count)];

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform sp = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            ActiveCursedObject.transform.position = sp.position;
            ActiveCursedObject.transform.rotation = sp.rotation;
        }

        ActiveCursedObject.ActivateObject();
        OnNewObjectSpawned?.Invoke(ActiveCursedObject);
    }

    public void OnSealSuccess()
    {
        if (ActiveCursedObject != null)
        {
            ActiveCursedObject.SealObject();
        }

        SealedObjectsCount++;
        OnObjectSealed?.Invoke(SealedObjectsCount);

        bool allSealed = SealedObjectsCount >= requiredSealedObjects;
        if (!allSealed && cursedObjectPool != null && cursedObjectPool.Count > 0)
        {
            allSealed = cursedObjectPool.FindAll(o => !o.IsSealed).Count == 0;
        }

        SetState(GameState.Playing);

        if (allSealed)
        {
            EscapeDoor doorComponent = GetEscapeDoorComponent();
            if (doorComponent != null) doorComponent.Unlock();
        }
        else
        {
            // Triggers 3-second delay before activating the next object
            ScheduleNextCursedObject();
        }
    }

    public void OnSealFailure(float penaltyAmount)
    {
        if (CandleController.Instance != null)
        {
            CandleController.Instance.ReduceCandlePower(30f);
        }

        SetState(GameState.Playing);

        if (ActiveCursedObject != null)
        {
            ActiveCursedObject.OnExorcismFailed();
        }
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);

        if (newState == GameState.MiniGame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (newState == GameState.Playing)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void TriggerLoss(string reason)
    {
        SetState(GameState.Lost);
        StopAllGameplayAudio();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Game Over: " + reason);
    }

    private void StopAllGameplayAudio()
    {
        if (tickingAudioSource != null && tickingAudioSource.isPlaying) tickingAudioSource.Stop();
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying) backgroundMusicSource.Stop();
        if (heartbeatAudioSource != null && heartbeatAudioSource.isPlaying) heartbeatAudioSource.Stop();
    }
}
