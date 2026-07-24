using UnityEngine;

public class Commit2Test : MonoBehaviour
{
    private void Update()
    {
        if (GameManager.Instance == null || SanityManager.Instance == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("[Test] Simulating Sealing a Cursed Object (Success: +20 Sanity, progresses count)...");
            GameManager.Instance.OnSealSuccess();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("[Test] Simulating Failing a Cursed Object (Failure: -15 Sanity penalty)...");
            GameManager.Instance.OnSealFailure(15f);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("[Test] Simulating Instant Sanity Collapse (Loss Condition)...");
            SanityManager.Instance.SubtractSanity(100f);
        }
        
        // Log status every 10 seconds to see timer decay in console
        if (Time.frameCount % 600 == 0)
        {
            Debug.Log($"[Test Status] Time Remaining: {GameManager.Instance.TimeRemaining:F1}s | Current Sanity: {SanityManager.Instance.CurrentSanity:F1} | Sealed: {GameManager.Instance.SealedObjectsCount}/{3}");
        }
    }
}
