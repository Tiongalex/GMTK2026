using UnityEngine;
using TMPro;

public class InteractionSystem : MonoBehaviour
{
    [Header("Raycast Config")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 1f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("UI Interaction Prompt")]
    [SerializeField] private GameObject interactionTextUI; // e.g. "Press E to Exorcise"

    [Header("Audio")]
    [SerializeField] private AudioSource interactAudioSource;
    [SerializeField] private AudioClip interactSound;

    private CursedObject currentTarget;
    private TMP_Text promptTextComponent;
    private string defaultPromptText = "Press E to Exorcise";

    // Prevents re-triggering until the key is fully released and pressed again
    private bool waitingForKeyRelease = false;

    private void Start()
    {
        if (interactionTextUI != null)
        {
            promptTextComponent = interactionTextUI.GetComponent<TMP_Text>();
            if (promptTextComponent != null)
            {
                defaultPromptText = promptTextComponent.text;
            }
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        // If not playing, hide UI and flag that we need a fresh key press when returning to Playing state
        if (GameManager.Instance.CurrentState != GameState.Playing)
        {
            if (interactionTextUI) interactionTextUI.SetActive(false);
            waitingForKeyRelease = true;
            return;
        }

        // If player just exited minigame by pressing E, wait until they RELEASE 'E' before allowing new interactions
        if (waitingForKeyRelease)
        {
            if (Input.GetKeyUp(KeyCode.E) || !Input.GetKey(KeyCode.E))
            {
                waitingForKeyRelease = false;
            }
            else
            {
                // Still holding 'E' from exiting minigame, skip input processing
                return;
            }
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        // Visual debug line in Scene View
        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red);

        if (Physics.Raycast(ray, out hit, interactRange, interactableLayer))
        {
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);

            // Check for CursedObject
            CursedObject cursed = hit.collider.GetComponent<CursedObject>();
            if (cursed == null)
            {
                cursed = hit.collider.GetComponentInParent<CursedObject>();
            }

            if (cursed != null && !cursed.IsSealed && cursed.IsActive)
            {
                currentTarget = cursed;
                if (interactionTextUI)
                {
                    if (promptTextComponent != null)
                    {
                        promptTextComponent.text = defaultPromptText;
                    }
                    interactionTextUI.SetActive(true);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    StartTalismanDrawing();
                }
                return;
            }

            // Check for EscapeDoor
            EscapeDoor door = hit.collider.GetComponent<EscapeDoor>();
            if (door == null)
            {
                door = hit.collider.GetComponentInParent<EscapeDoor>();
            }

            if (door != null)
            {
                currentTarget = null; // Clear cursed target
                if (interactionTextUI)
                {
                    if (promptTextComponent != null)
                    {
                        promptTextComponent.text = door.IsUnlocked ? "Press E to Escape" : "The door is locked";
                    }
                    interactionTextUI.SetActive(true);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    door.Interact();
                }
                return;
            }
        }

        currentTarget = null;
        if (interactionTextUI) interactionTextUI.SetActive(false);
    }

    private void StartTalismanDrawing()
    {
        AudioSource source = interactAudioSource;
        if (source == null)
        {
            source = GetComponent<AudioSource>();
            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
            }
        }

        if (source != null && interactSound != null)
        {
            source.PlayOneShot(interactSound);
        }

        if (interactionTextUI) interactionTextUI.SetActive(false);

        GameManager.Instance.SetState(GameState.MiniGame);
        if (TalismanMiniGame.Instance != null)
        {
            TalismanMiniGame.Instance.StartMiniGame(currentTarget);
        }
    }
}