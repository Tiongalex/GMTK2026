using UnityEngine;

public class CandleController : MonoBehaviour
{
    public static CandleController Instance { get; private set; }

    [SerializeField] private float candlePower = 100f;
    public float CandlePower => candlePower;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
