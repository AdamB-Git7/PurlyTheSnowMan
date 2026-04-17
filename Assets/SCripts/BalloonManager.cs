using System.Collections;
using UnityEngine;

/// <summary>
/// Manages balloon placement on the four arena walls.
/// At all times exactly four balloons are present (one per wall).
/// When a balloon is popped, a new one spawns on the SAME wall within 2 seconds
/// at a random position along that wall's edge.
/// </summary>
public class BalloonManager : MonoBehaviour
{
    // Global reference so popped balloons can ask the manager to respawn a replacement.
    public static BalloonManager Instance { get; private set; }

    [Header("Balloon Prefab")]
    // Prefab spawned on each arena wall.
    public GameObject balloonPrefab;

    [Header("Arena Bounds (world space)")]
    // The playable arena edges used to place balloons along the walls.
    public float arenaLeft = -12f;
    public float arenaRight = 12f;
    public float arenaTop = 6f;
    public float arenaBottom = -6f;

    [Tooltip("How far inset from the wall edge the balloon spawns")]
    // Spawn slightly inside the wall so the balloon is visible and reachable.
    public float wallOffset = 0.5f;

    // The latest a respawn is allowed to happen after a balloon is popped.
    private const float MaxRespawnDelay = 2f;

    void Awake()
    {
        // Keep exactly one active manager instance in the scene.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        // Remove any balloons that may have been left in the scene manually,
        // so the manager controls all balloon spawning consistently.
        try
        {
            foreach (GameObject balloon in GameObject.FindGameObjectsWithTag("Balloon"))
            {
                Destroy(balloon);
            }
        }
        catch (UnityException)
        {
            // Ignore the exception if the Balloon tag does not exist yet.
        }

        // Spawn exactly one balloon on each wall when the game begins.
        for (int wallIndex = 0; wallIndex < 4; wallIndex++)
        {
            SpawnBalloon(wallIndex);
        }
    }

    /// <summary>
    /// Called by BalloonPop when a balloon is popped.
    /// Respawns a new balloon on the same wall within 2 seconds.
    /// </summary>
    public void ScheduleRespawn(int wallIndex)
    {
        // Ignore invalid wall indices instead of spawning in a bad location.
        if (wallIndex < 0)
        {
            return;
        }

        // Delay the respawn to keep balloon pops readable during gameplay.
        StartCoroutine(RespawnAfterDelay(wallIndex));
    }

    private IEnumerator RespawnAfterDelay(int wallIndex)
    {
        // Wait a small random amount of time before replacing the popped balloon.
        yield return new WaitForSeconds(Random.Range(0.5f, MaxRespawnDelay));

        // Create the replacement balloon on the same wall.
        SpawnBalloon(wallIndex);
    }

    private void SpawnBalloon(int wallIndex)
    {
        // Do nothing if no prefab has been assigned in the Inspector.
        if (balloonPrefab == null)
        {
            return;
        }

        // Pick a valid spawn point along the requested wall.
        Vector3 position = GetRandomWallPosition(wallIndex);

        // Create the balloon at that position with no rotation.
        GameObject balloon = Instantiate(balloonPrefab, position, Quaternion.identity);

        // Ensure spawned balloons can be found and cleaned up by tag.
        balloon.tag = "Balloon";

        // Store the wall index on the balloon so it can respawn on the same wall later.
        BalloonPop pop = balloon.GetComponent<BalloonPop>();
        if (pop != null)
        {
            pop.wallIndex = wallIndex;
        }
    }

    /// <summary>
    /// Returns a random position along the given wall edge.
    /// 0 = top, 1 = bottom, 2 = left, 3 = right.
    /// </summary>
    private Vector3 GetRandomWallPosition(int wallIndex)
    {
        switch (wallIndex)
        {
            case 0:
                // Pick a random x-position along the top edge.
                return new Vector3(Random.Range(arenaLeft + 1f, arenaRight - 1f), arenaTop - wallOffset, 0f);
            case 1:
                // Pick a random x-position along the bottom edge.
                return new Vector3(Random.Range(arenaLeft + 1f, arenaRight - 1f), arenaBottom + wallOffset, 0f);
            case 2:
                // Pick a random y-position along the left edge.
                return new Vector3(arenaLeft + wallOffset, Random.Range(arenaBottom + 1f, arenaTop - 1f), 0f);
            case 3:
            default:
                // Pick a random y-position along the right edge.
                return new Vector3(arenaRight - wallOffset, Random.Range(arenaBottom + 1f, arenaTop - 1f), 0f);
        }
    }
}
