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

        // Force set state on start without triggering redundant material instantiations
        if (isActive)
        {
            ActivateObject(true);
        }
        else
        {
            DeactivateObject();
        }
    }

    public void ActivateObject(bool force = false)
    {
        // Prevents flickering if ActivateObject() is called while already active!
        if (isActive && !force) return;

        isActive = true;
        isSealed = false;

        if (curseParticles && !curseParticles.activeSelf)
            curseParticles.SetActive(true);

        if (objectRenderer && activeMaterial)
            objectRenderer.sharedMaterial = activeMaterial;

        if (curseAudioSource && activeScarySound)
        {
            curseAudioSource.loop = false; // Ensures it plays only once
            curseAudioSource.PlayOneShot(activeScarySound); // Plays cleanly without interrupting existing audio
        }
    }

    public void DeactivateObject()
    {
        isActive = false;
        if (curseParticles) curseParticles.SetActive(false);
        if (curseAudioSource) curseAudioSource.Stop();
        if (objectRenderer && originalMaterial) objectRenderer.sharedMaterial = originalMaterial;
    }

    public void SealObject()
    {
        isSealed = true;
        isActive = false;
        if (curseParticles) curseParticles.SetActive(false);
        if (objectRenderer && sealedMaterial) objectRenderer.sharedMaterial = sealedMaterial;

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