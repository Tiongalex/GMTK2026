using UnityEngine;

public class TalismanMiniGame : MonoBehaviour
{
    public static TalismanMiniGame Instance { get; private set; }

    private CursedObject currentObject;
    private bool miniGameActive = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartMiniGame(CursedObject obj)
    {
        currentObject = obj;
        miniGameActive = true;
        Debug.Log($"[Talisman Mini-Game Stub] Started mini-game on cursed object: {obj.gameObject.name}");
        Debug.Log("Press [Y] to simulate Success, [U] to simulate Failure in this stub.");
    }

    private void Update()
    {
        if (!miniGameActive || currentObject == null) return;

        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("[Test] Simulating Mini-Game SUCCESS...");
            currentObject.SealObject();
            miniGameActive = false;
            GameManager.Instance.OnSealSuccess();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("[Test] Simulating Mini-Game FAILURE...");
            miniGameActive = false;
            GameManager.Instance.OnSealFailure(currentObject.SanityFailurePenalty);
        }
    }
}
