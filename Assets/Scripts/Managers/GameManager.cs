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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        TimeRemaining = totalTime;
        SealedObjectsCount = 0;
        SpawnNextCursedObject();
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing && CurrentState != GameState.MiniGame) return;

        // Count down timer
        TimeRemaining -= Time.deltaTime;
        if (TimeRemaining <= 0)
        {
            TimeRemaining = 0;
            TriggerLoss("Time's Up");
        }
    }

    public void SpawnNextCursedObject()
    {
        if (SealedObjectsCount >= requiredSealedObjects)
        {
            TriggerWin();
            return;
        }

        // Disable previous active object
        if (ActiveCursedObject != null)
        {
            ActiveCursedObject.DeactivateObject();
        }

        // Randomly pick next object (not already sealed)
        List<CursedObject> available = cursedObjectPool.FindAll(o => !o.IsSealed);
        if (available.Count == 0) return;

        ActiveCursedObject = available[UnityEngine.Random.Range(0, available.Count)];
        
        // Randomly assign spawn position if spawnPoints provided
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
        
        // Unlock cursor for minigame, lock it for playing
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
        if (escapeDoor != null) escapeDoor.SetActive(false); // Unlocks door
        Debug.Log("Escape Room Unlocked! You Win!");
    }

    public void TriggerLoss(string reason)
    {
        SetState(GameState.Lost);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Game Over: " + reason);
    }
}
