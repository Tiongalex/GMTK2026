using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TalismanMiniGame : MonoBehaviour
{
    public static TalismanMiniGame Instance { get; private set; }

    [Header("UI Containers")]
    [SerializeField] private GameObject gameOverlayCanvas;
    [SerializeField] private RectTransform talismanPaper;
    [SerializeField] private RectTransform movingTargetPoint; // Magic tracking point
    [SerializeField] private Image targetPointVisual;
    [SerializeField] private Image successProgressBar;
    [SerializeField] private Image talismanMarkings; // Red calligraphy markings

    [Header("Gameplay Constants")]
    [SerializeField] private float successTimeRequired = 6.67f;
    [SerializeField] private float toleranceTimeAllowed = 1.67f;
    [SerializeField] private float targetSpeed = 150f;
    [SerializeField] private float targetRadius = 50f; // Range from center to track cursor
    [SerializeField] private float randomDirectionInterval = 0.5f;

    [Header("Visual Juice Settings")]
    [SerializeField] private Color trackingColor = new Color(0.1f, 0.7f, 1f, 1f); // Magic Blue
    [SerializeField] private Color searchingColor = new Color(1f, 0.2f, 0.2f, 1f); // Warning Red
    [SerializeField] private float pulseSpeed = 8f;
    [SerializeField] private float pulseMinScale = 0.85f;
    [SerializeField] private float pulseMaxScale = 1.15f;
    [SerializeField] private float minMarkingsAlpha = 0.15f;
    [SerializeField] private float maxMarkingsAlpha = 1.0f;

    [Header("Audio")]
    [SerializeField] private AudioSource miniGameAudioSource;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip failureSound;

    private CursedObject targetObject;
    private bool isRunning;
    private float currentSuccessTimer;
    private float currentToleranceTimer;
    private Vector2 moveDirection;
    private float dirTimer;
    private bool isTracking;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (gameOverlayCanvas) gameOverlayCanvas.SetActive(false);
    }

    public void StartMiniGame(CursedObject obj)
    {
        targetObject = obj;
        isRunning = true;
        currentSuccessTimer = 0f;
        currentToleranceTimer = 0f;
        isTracking = false;
        
        if (gameOverlayCanvas) gameOverlayCanvas.SetActive(true);
        
        // Reset point position to center of Paper and reset scale
        if (movingTargetPoint)
        {
            movingTargetPoint.anchoredPosition = Vector2.zero;
            movingTargetPoint.localScale = Vector3.one;
        }

        // Initialize red markings visibility to minimum starting alpha
        if (talismanMarkings)
        {
            Color c = talismanMarkings.color;
            c.a = minMarkingsAlpha;
            talismanMarkings.color = c;
        }

        SetRandomDirection();
    }

    private void Update()
    {
        if (!isRunning) return;

        UpdateTargetMovement();
        CheckPlayerTracking();
        UpdateVisualJuice();
    }

    private void UpdateTargetMovement()
    {
        if (movingTargetPoint == null || talismanPaper == null) return;

        dirTimer += Time.deltaTime;
        if (dirTimer >= randomDirectionInterval)
        {
            SetRandomDirection();
        }

        // Move target point
        Vector2 currentPos = movingTargetPoint.anchoredPosition;
        Vector2 nextPos = currentPos + moveDirection * targetSpeed * Time.deltaTime;

        // Keep inside bounds of the Talisman Paper Rect
        Vector2 paperSize = talismanPaper.rect.size * 0.4f; // 40% margin to prevent going off-screen
        nextPos.x = Mathf.Clamp(nextPos.x, -paperSize.x, paperSize.x);
        nextPos.y = Mathf.Clamp(nextPos.y, -paperSize.y, paperSize.y);

        // If hitting edge, bounce
        if (Mathf.Abs(nextPos.x) >= paperSize.x) moveDirection.x *= -1f;
        if (Mathf.Abs(nextPos.y) >= paperSize.y) moveDirection.y *= -1f;

        movingTargetPoint.anchoredPosition = nextPos;
    }

    private void SetRandomDirection()
    {
        moveDirection = Random.insideUnitCircle.normalized;
        dirTimer = 0f;
    }

    private void CheckPlayerTracking()
    {
        if (talismanPaper == null || movingTargetPoint == null) return;

        // Get mouse position relative to Canvas
        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            talismanPaper, 
            Input.mousePosition, 
            null, 
            out localMousePos
        );

        float distance = Vector2.Distance(localMousePos, movingTargetPoint.anchoredPosition);
        isTracking = (distance <= targetRadius);

        // Feedback color using dynamic tracking and searching colors
        if (targetPointVisual)
        {
            targetPointVisual.color = isTracking ? trackingColor : searchingColor;
        }

        if (isTracking)
        {
            currentSuccessTimer += Time.deltaTime;
            // Recover failure grace period slightly if you are tracking again
            currentToleranceTimer = Mathf.Max(0f, currentToleranceTimer - Time.deltaTime);
        }
        else
        {
            currentToleranceTimer += Time.deltaTime;
        }

        // Update progress bar UI
        if (successProgressBar)
        {
            successProgressBar.fillAmount = currentSuccessTimer / successTimeRequired;
        }

        // Win check
        if (currentSuccessTimer >= successTimeRequired)
        {
            CompleteExorcism();
        }
        // Failure check
        else if (currentToleranceTimer >= toleranceTimeAllowed)
        {
            FailExorcism();
        }
    }

    private void UpdateVisualJuice()
    {
        if (movingTargetPoint == null) return;

        // Pulse the tracking point's scale if tracking, otherwise reset to 1
        if (isTracking)
        {
            float pulse = Mathf.PingPong(Time.time * pulseSpeed, pulseMaxScale - pulseMinScale) + pulseMinScale;
            movingTargetPoint.localScale = new Vector3(pulse, pulse, 1f);
        }
        else
        {
            movingTargetPoint.localScale = Vector3.Lerp(movingTargetPoint.localScale, Vector3.one, Time.deltaTime * 5f);
        }

        // Dynamically fade in the red markings as progress increases
        if (talismanMarkings)
        {
            float progress = currentSuccessTimer / successTimeRequired;
            Color c = talismanMarkings.color;
            c.a = Mathf.Lerp(minMarkingsAlpha, maxMarkingsAlpha, progress);
            talismanMarkings.color = c;
        }
    }

    private void CompleteExorcism()
    {
        isRunning = false;
        if (gameOverlayCanvas) gameOverlayCanvas.SetActive(false);
        if (targetObject) targetObject.SealObject();
        
        if (miniGameAudioSource && successSound) miniGameAudioSource.PlayOneShot(successSound);

        if (GameManager.Instance) GameManager.Instance.OnSealSuccess();
    }

    private void FailExorcism()
    {
        isRunning = false;
        if (gameOverlayCanvas) gameOverlayCanvas.SetActive(false);
        
        if (miniGameAudioSource && failureSound) miniGameAudioSource.PlayOneShot(failureSound);

        float penalty = targetObject != null ? targetObject.SanityFailurePenalty : 15f;
        if (GameManager.Instance) GameManager.Instance.OnSealFailure(penalty);
    }
}
