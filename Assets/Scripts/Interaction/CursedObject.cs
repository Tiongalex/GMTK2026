using UnityEngine;

public class CursedObject : MonoBehaviour
{
    [Header("Object Identity")]
    [SerializeField] private string objectName;
    [SerializeField] private float sanityFailurePenalty = 15f;

    [Header("Visual & Audio Feedback")]
    [SerializeField] private GameObject curseParticles;
    [SerializeField] private AudioSource curseAudioSource;
    [SerializeField] private AudioClip activeScarySound;
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material sealedMaterial;
    [SerializeField] private Renderer objectRenderer;

    [Header("State")]
    [SerializeField] private bool isActive = false;
    [SerializeField] private bool isSealed = false;

    public bool IsActive => isActive;
    public bool IsSealed => isSealed;
    public float SanityFailurePenalty => sanityFailurePenalty;

    private Material originalMaterial;

    private void Start()
    {
        if (objectRenderer) originalMaterial = objectRenderer.sharedMaterial;
        if (isActive)
        {
            ActivateObject();
        }
        else
        {
            DeactivateObject();
        }
    }

    public void ActivateObject()
    {
        isActive = true;
        isSealed = false;

        if (curseParticles) curseParticles.SetActive(true);
        if (objectRenderer && activeMaterial) objectRenderer.material = activeMaterial;

        if (curseAudioSource && activeScarySound)
        {
            curseAudioSource.clip = activeScarySound;
            curseAudioSource.loop = true;
            curseAudioSource.volume = 1f;
            curseAudioSource.Play();
        }
    }

    public void DeactivateObject()
    {
        isActive = false;
        if (curseParticles) curseParticles.SetActive(false);
        if (curseAudioSource) curseAudioSource.Stop();
    }

    public void SealObject()
    {
        isSealed = true;
        isActive = false;
        if (curseParticles) curseParticles.SetActive(false);
        if (objectRenderer && sealedMaterial) objectRenderer.material = sealedMaterial;
        
        if (curseAudioSource)
        {
            curseAudioSource.Stop();
        }
    }

    public void OnExorcismFailed()
    {
        Debug.Log(objectName + " reacted violently to failed exorcism!");
    }
}
