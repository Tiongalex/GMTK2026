using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GhostAI : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 0.9f;
    [SerializeField] private float rotationSpeed = 90f;

    [Header("Candle Drain Settings")]
    [Tooltip("How much candle power to drain per second when touching the player")]
    [SerializeField] private float drainRatePerSecond = 10.0f;

    [Header("Whisper Audio Settings")]
    [SerializeField] private AudioClip[] whisperClips;
    [SerializeField] private float whisperDistanceThreshold = 3.0f;
    [SerializeField][Range(0f, 1f)] private float whisperVolume = 1.0f;

    [Header("Player Tracking")]
    [SerializeField] private string playerTag = "Player";

    private NavMeshAgent navAgent;
    private Transform playerTransform;
    private bool isTouchingPlayer = false;
    private bool hasWhispered = false;

    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();

        // Configure NavMeshAgent defaults
        navAgent.speed = moveSpeed;
        navAgent.angularSpeed = rotationSpeed;
        navAgent.stoppingDistance = 0f;

        // Find Player by Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("[GhostAI] Could not find GameObject with tag 'Player'!");
        }
    }

    private void Update()
    {
        // 1. Pathfind towards player
        if (playerTransform != null && navAgent.enabled && navAgent.isOnNavMesh)
        {
            navAgent.SetDestination(playerTransform.position);
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= whisperDistanceThreshold)
        {
            if (!hasWhispered)
            {
                PlayRandomWhisper();
                hasWhispered = true; // Ensures it plays only ONCE per approach
            }
        }
        else
        {
            // Reset whisper trigger if player escapes farther than the threshold
            hasWhispered = false;
        }

        // 2. Drain candle power constantly while touching player
        if (isTouchingPlayer)
        {
            DrainCandlePower();
        }
    }

    private void PlayRandomWhisper()
    {
        if (whisperClips != null && whisperClips.Length > 0)
        {
            int randomIndex = Random.Range(0, whisperClips.Length);
            AudioClip chosenWhisper = whisperClips[randomIndex];

            if (chosenWhisper != null)
            {
                // Play 3D audio at the ghost's position
                AudioSource.PlayClipAtPoint(chosenWhisper, transform.position, whisperVolume);
            }
        }
    }

    private void DrainCandlePower()
    {
        if (CandleController.Instance != null)
        {
            // Subtract power over time (5 units per second)
            float amountToDrain = drainRatePerSecond * Time.deltaTime;
            CandleController.Instance.DrainPower(amountToDrain);
        }
    }

    // --- Collision & Trigger Contact Handling ---

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            isTouchingPlayer = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            isTouchingPlayer = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isTouchingPlayer = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isTouchingPlayer = false;
        }
    }

    /// <summary>
    /// Call this from GhostVanishTrigger when ghost starts fading to stop AI pathfinding.
    /// </summary>
    public void StopAI()
    {
        isTouchingPlayer = false;
        if (navAgent != null && navAgent.enabled)
        {
            navAgent.isStopped = true;
            navAgent.enabled = false;
        }
    }
}