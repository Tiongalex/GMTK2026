using System.Collections;
using UnityEngine;

public class GhostVanishTrigger : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("How directly the player must look at ghost (0.5 = wide screen, 0.85 = centered on crosshair)")]
    [SerializeField] private float viewDotThreshold = 0.7f;

    [Tooltip("Minimum candle light intensity required for light to affect the ghost")]
    [SerializeField] private float minLightIntensity = 1.0f;

    [Tooltip("How long (in seconds) the light must hit the ghost before it begins vanishing")]
    [SerializeField] private float requiredExposureTimeToVanish = 2.0f;

    [Header("Vanish / Fade Settings")]
    [Tooltip("How long it takes for the ghost to completely fade out to alpha 0")]
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip zombieMoanSFX;
    [Range(0f, 1f)][SerializeField] private float moanVolume = 1.0f;

    [Header("Layer Setup")]
    [SerializeField] private LayerMask obstacleLayers;

    private Transform playerCamera;
    private Light candlePointLight;
    private float exposureTimer = 0f;
    private bool isVanishing = false;
    private bool hasPlayedMoan = false;
    private Renderer[] ghostRenderers;

    private void Start()
    {
        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        ghostRenderers = GetComponentsInChildren<Renderer>();
    }

    private void Update()
    {
        if (isVanishing || playerCamera == null || CandleController.Instance == null) return;

        if (IsLitAndTargeted())
        {
            exposureTimer += Time.deltaTime;

            // Trigger moan sound once when exposure starts
            if (!hasPlayedMoan && zombieMoanSFX != null)
            {
                // Spawns a temporary 3D audio source at the ghost's position that survives Destroy()
                AudioSource.PlayClipAtPoint(zombieMoanSFX, transform.position, moanVolume);
                hasPlayedMoan = true;
            }

            if (exposureTimer >= requiredExposureTimeToVanish)
            {
                StartCoroutine(VanishAndFadeRoutine());
            }
        }
        else
        {
            // Reset timer if player looks away
            exposureTimer = Mathf.Max(0f, exposureTimer - Time.deltaTime);

            if (exposureTimer == 0f)
            {
                hasPlayedMoan = false; // Allow moan to trigger again if player re-engages
            }
        }
    }

    private bool IsLitAndTargeted()
    {
        if (CandleController.Instance.CandlePower <= 0) return false;

        Light lightComp = GetCandleLight();
        if (lightComp == null || lightComp.intensity < minLightIntensity) return false;

        Vector3 dirToGhost = transform.position - playerCamera.position;
        float distanceToGhost = dirToGhost.magnitude;

        if (distanceToGhost > lightComp.range) return false;

        Vector3 normalizeDir = dirToGhost.normalized;
        float dot = Vector3.Dot(playerCamera.forward, normalizeDir);

        if (dot < viewDotThreshold) return false;

        if (Physics.Raycast(playerCamera.position, normalizeDir, out RaycastHit hit, distanceToGhost, obstacleLayers))
        {
            if (hit.transform != transform && !hit.transform.IsChildOf(transform))
            {
                return false;
            }
        }

        return true;
    }

    private Light GetCandleLight()
    {
        if (candlePointLight == null && CandleController.Instance != null)
        {
            candlePointLight = CandleController.Instance.GetComponentInChildren<Light>();
        }
        return candlePointLight;
    }

    private IEnumerator VanishAndFadeRoutine()
    {
        isVanishing = true;

        // Stop Ghost AI pathfinding and contact drains
        GhostAI ghostAI = GetComponent<GhostAI>();
        if (ghostAI != null)
        {
            ghostAI.StopAI();
        }

        // 1. Freeze Physics so ghost doesn't fall through floor
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        // 2. Disable Colliders
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        float elapsedTime = 0f;

        // 3. Smooth Alpha Fade
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

            SetGhostAlpha(alpha);
            yield return null;
        }

        SetGhostAlpha(0f);

        // Destroy ghost mesh — the moan audio will continue playing seamlessly to the end!
        Destroy(gameObject);
    }

    private void SetGhostAlpha(float alpha)
    {
        foreach (Renderer rend in ghostRenderers)
        {
            if (rend == null) continue;

            foreach (Material mat in rend.materials)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color color = mat.color;
                    color.a = alpha;
                    mat.color = color;
                }
                else if (mat.HasProperty("_BaseColor"))
                {
                    Color color = mat.GetColor("_BaseColor");
                    color.a = alpha;
                    mat.SetColor("_BaseColor", color);
                }
            }
        }
    }
}