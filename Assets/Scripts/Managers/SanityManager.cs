using UnityEngine;

public class SanityManager : MonoBehaviour
{
    public static SanityManager Instance { get; private set; }

    [Header("Sanity Configuration")]
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private float startingSanity = 100f;

    [Header("Passive Decay Rates (Sanity/sec)")]
    [SerializeField] private float phase1Decay = 0.5f; // 100s -> 60s
    [SerializeField] private float phase2Decay = 1.0f; // 60s -> 10s
    [SerializeField] private float phase3Decay = 3.0f; // 10s -> 0s

    public float CurrentSanity { get; private set; }
    public float MaxSanity => maxSanity;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        CurrentSanity = startingSanity;
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing && 
            GameManager.Instance.CurrentState != GameState.MiniGame) return;

        float time = GameManager.Instance.TimeRemaining;
        float decayRate = phase1Decay;

        if (time <= 10f)
        {
            decayRate = phase3Decay;
        }
        else if (time <= 60f)
        {
            decayRate = phase2Decay;
        }

        // Double decay rate if player is in total darkness (0% candle power)
        if (CandleController.Instance != null && CandleController.Instance.CandlePower <= 0f)
        {
            decayRate += 5.0f; // Extremely rapid decay in dark
        }

        SubtractSanity(decayRate * Time.deltaTime);
    }

    public void AddSanity(float amount)
    {
        CurrentSanity = Mathf.Clamp(CurrentSanity + amount, 0f, maxSanity);
    }

    public void SubtractSanity(float amount)
    {
        CurrentSanity = Mathf.Clamp(CurrentSanity - amount, 0f, maxSanity);
        if (CurrentSanity <= 0)
        {
            GameManager.Instance.TriggerLoss("Sanity collapsed.");
        }
    }
}
