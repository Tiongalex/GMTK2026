using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    [Header("Raycast Config")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("UI Interaction Prompt")]
    [SerializeField] private GameObject interactionTextUI; // e.g. "Press E to Exorcise"

    private CursedObject currentTarget;

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameState.Playing)
        {
            if (interactionTextUI) interactionTextUI.SetActive(false);
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        // Visual debug line in Scene View
        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red);

        if (Physics.Raycast(ray, out hit, interactRange, interactableLayer))
        {
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
            CursedObject cursed = hit.collider.GetComponent<CursedObject>();
            if (cursed != null && !cursed.IsSealed && cursed.IsActive)
            {
                currentTarget = cursed;
                if (interactionTextUI) interactionTextUI.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    StartTalismanDrawing();
                }
                return;
            }
        }

        currentTarget = null;
        if (interactionTextUI) interactionTextUI.SetActive(false);
    }

    private void StartTalismanDrawing()
    {
        GameManager.Instance.SetState(GameState.MiniGame);
        if (TalismanMiniGame.Instance != null)
        {
            TalismanMiniGame.Instance.StartMiniGame(currentTarget);
        }
    }
}
