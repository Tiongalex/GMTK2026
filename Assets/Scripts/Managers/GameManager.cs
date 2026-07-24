using System;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private float maxVolume = 1.0f; // Peak volume at 100% time elapsed

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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        TimeRemaining = totalTime;
        SealedObjectsCount = 0;

        // FIX 1: Initialize lights on Start
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

        // FIX 2: Call lighting update on every frame
        UpdateRoomLighting();

        if (TimeRemaining <= 0)
        {
            TimeRemaining = 0;
            TriggerLoss("Time's Up");
        }
    }

    private void UpdateTickingAudio()
    {
        if (tickingAudioSource == null || totalTime <= 0f) return;

        // Calculate progress ratio (0.0 at game start -> 1.0 when time hits zero)
        float elapsedRatio = Mathf.Clamp01((totalTime - TimeRemaining) / totalTime);

        // Amplitude (Volume) increases from 0 to maxVolume
        tickingAudioSource.volume = elapsedRatio * maxVolume;

        // FIX 3: Targeted tickingAudioSource for pitch acceleration (80%-100% timeframe)
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
            // Smooth noise value between 0.0 and 1.0 sampled over time
            float rawNoise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);

            // Flickering scales in severity as time elapses (0 at start -> maxFlickerSeverity at time's up)
            float currentSeverity = elapsedRatio * maxFlickerSeverity;

            // Only dip intensity when noise pushes past a threshold
            if (rawNoise > (1.0f - currentSeverity))
            {
                // Smoothly dip brightness instead of hard off/on snapping
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

    public void SpawnNextCursedObject()
    {
        if (SealedObjectsCount >= requiredSealedObjects)
        {
            TriggerWin();
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

        if (SealedObjectsCount >= requiredSealedObjects)
        {
            TriggerWin();
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
        StopBackgroundMusic();
        StopTickingAudio();
        if (escapeDoor != null) escapeDoor.SetActive(false);
        Debug.Log("Escape Room Unlocked! You Win!");
    }

    public void TriggerLoss(string reason)
    {
        SetState(GameState.Lost);
        StopBackgroundMusic();
        StopTickingAudio();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Game Over: " + reason);
    }

    private void StopTickingAudio()
    {
        if (tickingAudioSource != null && tickingAudioSource.isPlaying)
        {
            tickingAudioSource.Stop();
        }
    }

    private void StopBackgroundMusic()
    {
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Stop();
        }
    }
}