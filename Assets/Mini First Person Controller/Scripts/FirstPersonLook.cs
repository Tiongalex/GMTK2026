using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField]
    Transform character;
    public float sensitivity = 2;
    public float smoothing = 1.5f;

    [Header("Darkness Jitter Settings")]
    [SerializeField] private float maxJitterIntensity = 1.8f; // Max rotation offset in degrees when dark
    [SerializeField] private float jitterSpeed = 15f;          // Speed of trembling

    Vector2 velocity;
    Vector2 frameVelocity;

    void Reset()
    {
        // Get the character from the FirstPersonMovement in parents.
        character = GetComponentInParent<FirstPersonMovement>().transform;
    }

    void Start()
    {
        // Lock the mouse cursor to the game screen.
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 1. Stop processing and clear rotation offsets during minigames or menus
        if ((GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) || Cursor.lockState != CursorLockMode.Locked)
        {
            frameVelocity = Vector2.zero;
            // Ensure camera local rotation resets cleanly without jitter offset
            transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
            return;
        }

        // 2. Get raw mouse movement (PURE player input)
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        // 3. Smooth player input velocity
        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1f / smoothing);
        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -90, 90);

        // 4. Calculate isolated shake offset (does NOT touch `velocity`)
        Vector2 jitterOffset = CalculateCandleJitter();

        // 5. Apply base look rotation PLUS temporary jitter offset to the camera
        Quaternion baseLook = Quaternion.AngleAxis(-velocity.y, Vector3.right);
        Quaternion jitterRot = Quaternion.Euler(-jitterOffset.y, jitterOffset.x, 0f);

        transform.localRotation = baseLook * jitterRot;
        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
    }

    /// <summary>
    /// Generates balanced oscillatory jitter offsets using Sine and Perlin noise.
    /// </summary>
    private Vector2 CalculateCandleJitter()
    {
        if (CandleController.Instance == null) return Vector2.zero;

        float lightRatio = CandleController.Instance.CandlePower / CandleController.Instance.MaxCandlePower;
        float jitterStrength = (1f - lightRatio) * maxJitterIntensity;

        if (jitterStrength <= 0.01f) return Vector2.zero;

        // Combine Perlin noise with Sine waves on separate seeds to guarantee multi-directional tremble around 0
        float noiseX = (Mathf.PerlinNoise(Time.time * jitterSpeed, 10f) - 0.5f) * 2f + Mathf.Sin(Time.time * jitterSpeed * 1.5f) * 0.3f;
        float noiseY = (Mathf.PerlinNoise(20f, Time.time * jitterSpeed) - 0.5f) * 2f + Mathf.Cos(Time.time * jitterSpeed * 1.5f) * 0.3f;

        return new Vector2(noiseX * jitterStrength, noiseY * jitterStrength);
    }
}