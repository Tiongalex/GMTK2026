using System.Collections.Generic;
using UnityEngine;

public class GhostSpawner : MonoBehaviour
{
    public static GhostSpawner Instance { get; private set; }

    [Header("Ghost Setup")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField, Range(0f, 100f)] private float spawnChancePercent = 30f;

    [Header("Registered Rooms")]
    [SerializeField] private List<RoomDetector> allRooms = new List<RoomDetector>();

    // Public reference to access the active ghost instance later for AI scripts
    public GameObject ActiveGhost { get; private set; }
    public bool IsGhostSpawned => ActiveGhost != null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Called by RoomDetector whenever the player enters a room.
    /// </summary>
    public void OnPlayerEnterRoom(RoomDetector currentRoom)
    {
        // 1. If ghost is already spawned, do nothing
        if (IsGhostSpawned) return;

        // 2. Roll for 30% spawn chance
        float roll = Random.Range(0f, 100f);
        Debug.Log($"Player entered {currentRoom.roomId}. Roll: {roll:F1}% (Needs <= {spawnChancePercent}%)");

        if (roll <= spawnChancePercent)
        {
            SpawnGhostOutsideRoom(currentRoom);
        }
    }

    private void SpawnGhostOutsideRoom(RoomDetector playerCurrentRoom)
    {
        // Get all rooms EXCEPT the one the player just entered
        List<RoomDetector> otherRooms = allRooms.FindAll(r => r != playerCurrentRoom);

        if (otherRooms.Count == 0)
        {
            Debug.LogWarning("No other rooms available to spawn the ghost!");
            return;
        }

        // Pick a random room from the other rooms
        RoomDetector targetRoom = otherRooms[Random.Range(0, otherRooms.Count)];

        if (targetRoom.roomSpawnPoints == null || targetRoom.roomSpawnPoints.Length == 0)
        {
            Debug.LogWarning($"Room {targetRoom.roomId} has no spawn points assigned!");
            return;
        }

        // Pick a random spawn point inside that room
        Transform spawnPoint = targetRoom.roomSpawnPoints[Random.Range(0, targetRoom.roomSpawnPoints.Length)];

        // Spawn the ghost
        ActiveGhost = Instantiate(ghostPrefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log($"<color=red>Ghost spawned in {targetRoom.roomId} at {spawnPoint.name}!</color>");
    }
}