using UnityEngine;

public class Commit4Test : MonoBehaviour
{
    private void Update()
    {
        if (GameManager.Instance == null) return;

        // If in MiniGame state (which happens when you interact with a cursed object)
        if (GameManager.Instance.CurrentState == GameState.MiniGame)
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                Debug.Log("[Test] Simulating Mini-Game SUCCESS...");
                // In a real run, the active object is sealed
                if (GameManager.Instance.ActiveCursedObject != null)
                {
                    GameManager.Instance.ActiveCursedObject.SealObject();
                }
                GameManager.Instance.OnSealSuccess();
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                Debug.Log("[Test] Simulating Mini-Game FAILURE...");
                float penalty = GameManager.Instance.ActiveCursedObject != null ? 
                    GameManager.Instance.ActiveCursedObject.SanityFailurePenalty : 15f;
                GameManager.Instance.OnSealFailure(penalty);
            }
        }
    }
}
