using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Hover Scale Settings")]
    [SerializeField] private Vector3 normalScale = Vector3.one;
    [SerializeField] private Vector3 hoverScale = new Vector3(1.05f, 1.05f, 1.05f);
    [SerializeField] private Vector3 pressedScale = new Vector3(0.95f, 0.95f, 0.95f);
    [SerializeField] private float animationSpeed = 15f;

    [Header("Audio SFX")]
    [SerializeField] private AudioClip hoverSound;
    [Range(0f, 1f)] [SerializeField] private float hoverVolume = 0.4f;
    
    [SerializeField] private AudioClip clickSound;
    [Range(0f, 1f)] [SerializeField] private float clickVolume = 0.6f;

    [SerializeField] private AudioSource audioSource;

    private Vector3 targetScale;

    private void Awake()
    {
        targetScale = normalScale;

        // Auto-find or add AudioSource if sounds are assigned but no AudioSource reference was given
        if (audioSource == null && (hoverSound != null || clickSound != null))
        {
            if (!TryGetComponent<AudioSource>(out audioSource))
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }

    private void OnEnable()
    {
        transform.localScale = normalScale;
        targetScale = normalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = hoverScale;

        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound, hoverVolume);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = normalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = hoverScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound, clickVolume);
        }
    }
}
