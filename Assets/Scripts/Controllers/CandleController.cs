using UnityEngine;

public class CandleController : MonoBehaviour
{
    public static CandleController Instance { get; private set; }

    [Header("Power Settings")]
    [SerializeField] private float maxCandlePower = 100f;
    [SerializeField] private float currentCandlePower = 100f;

    [Header("Light Settings")]
    [SerializeField] private Light candleLight;
    [SerializeField] private float maxLightIntensity = 3f; // Target intensity when power is 100%

    [Header("Testing Controls")]
    [SerializeField] private bool enableTestDrain = false;
    [SerializeField] private float testDrainInterval = 2f; // Time in seconds between drains
    [SerializeField] private float testDrainAmount = 5f;    // Power removed per interval

    private float testTimer;

    // Public read-only accessors
    public float CandlePower => currentCandlePower;
    public float MaxCandlePower => maxCandlePower;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Ensure current starts capped at max
        currentCandlePower = maxCandlePower;

        // Auto-assign Light if not manually set in Inspector
        if (candleLight == null)
        {
            candleLight = GetComponent<Light>();
        }
    }

    private void Update()
    {
        // 1. Testing Drain Timer
        if (enableTestDrain && currentCandlePower > 0)
        {
            testTimer += Time.deltaTime;
            if (testTimer >= testDrainInterval)
            {
                DrainPower(testDrainAmount);
                testTimer = 0f;
            }
        }

        // 2. Real-time Light Intensity Update:
        // Formula: (current / max) * maxIntensity
        if (candleLight != null)
        {
            float powerPercentage = currentCandlePower / maxCandlePower;
            candleLight.intensity = powerPercentage * maxLightIntensity;
        }
    }

    /// <summary>
    /// Reduces candle power by a specified amount (used by test timer and future enemy attacks).
    /// </summary>
    public void DrainPower(float amount)
    {
        currentCandlePower -= amount;
        currentCandlePower = Mathf.Max(currentCandlePower, 0f); // Clamp so it doesn't go negative

        Debug.Log($"Candle Power: {currentCandlePower}/{maxCandlePower} | Light Intensity: {candleLight.intensity:F2}");
    }

    /// <summary>
    /// Call this later if you want items/matches to restore candle power.
    /// </summary>
    public void RestorePower(float amount)
    {
        currentCandlePower += amount;
        currentCandlePower = Mathf.Min(currentCandlePower, maxCandlePower);
    }
}