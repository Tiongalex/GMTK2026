using UnityEngine;

public class EscapeDoor : MonoBehaviour
{
    [Header("Audio Feedback")]
    [SerializeField] private AudioSource doorAudioSource;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip lockedRattleSound;

    [Header("Visual Feedback (Optional)")]
    [SerializeField] private GameObject lockedIndicator;
    [SerializeField] private GameObject unlockedIndicator;

    private bool isUnlocked = false;

    public bool IsUnlocked => isUnlocked;

    private void Start()
    {
        // Auto-assign AudioSource if not set or missing
        if (doorAudioSource == null)
        {
            doorAudioSource = GetComponent<AudioSource>();
            if (doorAudioSource == null)
            {
                doorAudioSource = GetComponentInChildren<AudioSource>();
            }
            if (doorAudioSource == null)
            {
                doorAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        UpdateVisuals();
    }

    public void Unlock()
    {
        if (isUnlocked) return;

        isUnlocked = true;
        UpdateVisuals();

        if (doorAudioSource != null && unlockSound != null)
        {
            doorAudioSource.PlayOneShot(unlockSound);
        }

        Debug.Log("[EscapeDoor] Door is now unlocked!");
    }

    public void Interact()
    {
        if (isUnlocked)
        {
            Debug.Log("[EscapeDoor] Player interacted with unlocked escape door. Winning game!");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerWin();
            }
        }
        else
        {
            Debug.Log("[EscapeDoor] Player interacted with locked escape door.");
            if (doorAudioSource != null && lockedRattleSound != null)
            {
                // Only play if the audio source is NOT currently playing
                if (!doorAudioSource.isPlaying)
                {
                    doorAudioSource.PlayOneShot(lockedRattleSound);
                    Debug.Log("Played");
                }
            }
        }
    }

    private void UpdateVisuals()
    {
        if (lockedIndicator != null) lockedIndicator.SetActive(!isUnlocked);
        if (unlockedIndicator != null) unlockedIndicator.SetActive(isUnlocked);
    }
}
