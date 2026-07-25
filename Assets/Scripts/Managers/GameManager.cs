using System; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField] private GameObject escapeDoor; // Unlocks on win

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
    [SerializeField] private float minPitch = 1.0f; // Normal speed at start (0% time elapsed)
    [SerializeField] private float maxPitch = 1.0f;
    [SerializeField] private float maxVolume = 2.0f; // Peak volume at 100% time elapsed

    [Header("Lighting Settings")]
    [SerializeField] private List<Light> roomLights = new List<Light>();
    [SerializeField] private Color normalColor = new Color(212f / 255f, 100f / 255f, 40f / 255f, 1f); // Warm orange/amber starting color
    [SerializeField] private Color corruptedColor = new Color(0.396f, 0f, 0f, 1f); // Deep red/orange
    [SerializeField] private float minIntensityMultiplier = 0.4f; // Dimness near time's up
    [SerializeField] private bool enableFlicker = true; // Enables electrical flicker past 80% time

    [Header("Flicker Tuning")]
    [SerializeField] private float flickerSpeed = 9f; // Slower speed = realistic voltage drops (was 18f)
    [SerializeField] private float maxFlickerSeverity = 0.9f; // Max dip depth near time's up

    private Dictionary<Light, float> originalIntensities = new Dictionary<Light, float>();

    [Header("Heartbeat Settings")]
    [SerializeField] private AudioSource heartbeatAudioSource;
    [SerializeField] private float heartbeatTriggerTime = 30f; // Seconds remaining to trigger

    private bool heartbeatStarted = false;
    private bool deathSequenceTriggered = false; // Flag to prevent multi-triggering coroutine

    [Header("Zero-Second Sequence Audio")]
    [SerializeField] private AudioSource doorOpenAudioSource;
    [SerializeField] private AudioSource jumpscareAudioSource;

    [Header("UI Death Fade")]
    [SerializeField] private CanvasGroup fadeScreenCanvasGroup; // CanvasGroup on a full-screen black UI Image
    [SerializeField] private float fadeDuration = 0.5f; // Duration of black screen fade

    [Header("Player Control")]
    [SerializeField] private MonoBehaviour playerControllerScript; // Drag FirstPersonController / Player Movement script here

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        TimeRemaining = totalTime;
        SealedObjectsCount = 0;

        // Auto-assign escapeDoor if not set in inspector
        if (escapeDoor == null)
        {
            EscapeDoor door = FindObjectOfType<EscapeDoor>();
            if (door != null)
            {
                escapeDoor = door.gameObject;
            }
        }

        // Auto-assign fadeScreenCanvasGroup if not set in inspector
        if (fadeScreenCanvasGroup == null)
        {
            GameObject blackObj = GameObject.Find("Black");
            if (blackObj != null)
            {
                fadeScreenCanvasGroup = blackObj.GetComponent<CanvasGroup>();
            }
            if (fadeScreenCanvasGroup == null)
            {
                fadeScreenCanvasGroup = FindObjectOfType<CanvasGroup>();
            }
        }

        // Auto-populate cursedObjectPool if empty in inspector
        if (cursedObjectPool == null || cursedObjectPool.Count == 0)
        {
            cursedObjectPool = new List<CursedObject>(FindObjectsOfType<CursedObject>());
        }

        // Clamp requiredSealedObjects to available count
        if (cursedObjectPool.Count > 0)
        {
            requiredSealedObjects = Mathf.Min(requiredSealedObjects, cursedObjectPool.Count);
        }

        InitializeLighting();
        SpawnNextCursedObject();

        // Start Ambient Background Music
        if (backgroundMusicSource != null && !backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.loop = true;
            backgroundMusicSource.Play();
        }

        // Start Ticking Audio Source
        if (tickingAudioSource != null)
        {
            tickingAudioSource.loop = true;
            tickingAudioSource.volume = 0f; // Start at 0 volume
            tickingAudioSource.pitch = minPitch;
            tickingAudioSource.Play();
        }
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing && CurrentState != GameState.MiniGame) return;

        // Count down timer
        TimeRemaining -= Time.deltaTime;

        // Dynamic Progression
        UpdateTickingAudio();
        UpdateRoomLighting();

        if (TimeRemaining <= 0 && !deathSequenceTriggered)
        {
            deathSequenceTriggered = true;
            TimeRemaining = 0;
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

    private IEnumerator DeathSequenceCoroutine()
    {
        // Lock player movement & camera
        if (playerControllerScript != null)
        {
            playerControllerScript.enabled = false;
        }
        // --- Step 1: At t = 0s ---
        // Play the slow door opening sound
        if (doorOpenAudioSource != null)
        {
            doorOpenAudioSource.Play();
        }

        // Wait exactly 2 seconds while the door sound plays/finishes
        yield return new WaitForSeconds(2.0f);

        // --- Step 2: At t = 2s ---
        // Play the jumpscare audio
        if (jumpscareAudioSource != null)
        {
            jumpscareAudioSource.Play();
        }

        // Simultaneously fade the screen to black
        if (fadeScreenCanvasGroup != null)
        {
            fadeScreenCanvasGroup.gameObject.SetActive(true);
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                fadeScreenCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }
            fadeScreenCanvasGroup.alpha = 1f; // Ensure solid black
        }
    }

    private void UpdateTickingAudio()
    {
        if (tickingAudioSource == null || totalTime <= 0f) return;

        // Calculate progress ratio (0.0 at game start -> 1.0 when time hits zero)
        float elapsedRatio = Mathf.Clamp01((totalTime - TimeRemaining) / totalTime);

        // Amplitude (Volume) increases from 0 to maxVolume
        heartbeatAudioSource.volume = elapsedRatio * maxVolume;

        // Pitch acceleration
        float pitchProgress = Mathf.InverseLerp(0.8f, 1.0f, elapsedRatio);
        backgroundMusicSource.pitch = Mathf.Lerp(minPitch, maxPitch, pitchProgress);
    }

    #region Lighting Logic

    private void InitializeLighting()
    {
        // Auto-find tagged lights if non-assigned
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

        // Save initial brightness of each light
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

        // Shift color toward corrupted color as time runs out
        Color currentColor = Color.Lerp(normalColor, corruptedColor, elapsedRatio);

        // Dim lights overall
        float intensityFactor = Mathf.Lerp(1.0f, minIntensityMultiplier, elapsedRatio);

        // Progressive flicker starting at t > 0
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

        // Apply settings to scene lights
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
        if (escapeDoor == null)
        {
            return FindObjectOfType<EscapeDoor>();
        }
        EscapeDoor door = escapeDoor.GetComponent<EscapeDoor>();
        if (door == null) door = escapeDoor.GetComponentInChildren<EscapeDoor>();
        if (door == null) door = escapeDoor.GetComponentInParent<EscapeDoor>();
        if (door == null) door = FindObjectOfType<EscapeDoor>();
        return door;
    }

    public void SpawnNextCursedObject()
    {
        bool allSealed = SealedObjectsCount >= requiredSealedObjects;
        if (!allSealed && cursedObjectPool != null && cursedObjectPool.Count > 0)
        {
            allSealed = cursedObjectPool.FindAll(o => !o.IsSealed).Count == 0;
        }

        if (allSealed)
        {
            EscapeDoor doorComponent = GetEscapeDoorComponent();
            if (doorComponent != null)
            {
                doorComponent.Unlock();
            }
            return;
        }

        if (ActiveCursedObject != null)
        {
            ActiveCursedObject.DeactivateObject();
        }

        List<CursedObject> available = cursedObjectPool.FindAll(o => !o.IsSealed);
        if (available.Count == 0) return;

        ActiveCursedObject = available[UnityEngine.Random.Range(0, available.Count)];

        if (spawnPoints.Length > 0)
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
        SealedObjectsCount++;
        OnObjectSealed?.Invoke(SealedObjectsCount);
        SanityManager.Instance.AddSanity(20f);

        bool allSealed = SealedObjectsCount >= requiredSealedObjects;
        if (!allSealed && cursedObjectPool != null && cursedObjectPool.Count > 0)
        {
            allSealed = cursedObjectPool.FindAll(o => !o.IsSealed).Count == 0;
        }

        if (allSealed)
        {
            SetState(GameState.Playing);
            EscapeDoor doorComponent = GetEscapeDoorComponent();
            if (doorComponent != null)
            {
                doorComponent.Unlock();
            }
            else
            {
                Debug.LogWarning("[GameManager] Could not find any EscapeDoor component in the scene!");
            }
        }
        else
        {
            SetState(GameState.Playing);
            SpawnNextCursedObject();
        }
    }

    public void OnSealFailure(float sanityPenalty)
    {
        SanityManager.Instance.SubtractSanity(sanityPenalty);
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

    public void TriggerWin()
    {
        SetState(GameState.Won);
        StopAllGameplayAudio();
        if (escapeDoor != null) escapeDoor.SetActive(false);
        Debug.Log("[GameManager] TriggerWin called. Escape Room Unlocked! You Win!");
        
        if (fadeScreenCanvasGroup == null)
        {
            GameObject blackObj = GameObject.Find("Black");
            if (blackObj != null)
            {
                fadeScreenCanvasGroup = blackObj.GetComponent<CanvasGroup>();
            }
            if (fadeScreenCanvasGroup == null)
            {
                fadeScreenCanvasGroup = FindObjectOfType<CanvasGroup>();
            }
        }

        Debug.Log("[GameManager] fadeScreenCanvasGroup is: " + (fadeScreenCanvasGroup != null ? fadeScreenCanvasGroup.name : "NULL"));
        StartCoroutine(WinSequenceCoroutine());
    }

    private IEnumerator WinSequenceCoroutine()
    {
        // Lock player movement & camera
        if (playerControllerScript != null)
        {
            playerControllerScript.enabled = false;
        }

        // Enable Cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (fadeScreenCanvasGroup != null)
        {
            fadeScreenCanvasGroup.gameObject.SetActive(true);

            // Remove any existing WinText children just in case
            Transform existing = fadeScreenCanvasGroup.transform.Find("WinText");
            if (existing != null) Destroy(existing.gameObject);

            // Create WinText GameObject
            GameObject winTextObj = new GameObject("WinText", typeof(RectTransform));
            winTextObj.transform.SetParent(fadeScreenCanvasGroup.transform, false);

            TextMeshProUGUI winText = winTextObj.AddComponent<TextMeshProUGUI>();
            winText.text = "Game Win";
            winText.fontSize = 64;
            winText.color = Color.white;
            winText.alignment = TextAlignmentOptions.Center;

            RectTransform rect = winTextObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            // Fade the screen to black
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                fadeScreenCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }
            fadeScreenCanvasGroup.alpha = 1f; // Ensure solid black
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