using UnityEngine;

public class RoomDetector : MonoBehaviour
{
    [Header("Room Setup")]
    [Tooltip("Unique ID or name for this room (e.g., Room1, Room2, Room3)")]
    public string roomId;

    [Tooltip("Possible spawn points for the ghost inside/near this specific room")]
    public Transform[] roomSpawnPoints;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is the player
        if (other.CompareTag("Player"))
        {
            if (GhostSpawner.Instance != null)
            {
                GhostSpawner.Instance.OnPlayerEnterRoom(this);
            }
        }
    }
}