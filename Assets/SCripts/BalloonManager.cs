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
    public static BalloonManager Instance { get; private set; }

    [Header("Balloon Prefab")]
    public GameObject balloonPrefab;

    [Header("Arena Bounds (world space)")]
    public float arenaLeft   = -12f;
    public float arenaRight  =  12f;
    public float arenaTop    =   6f;
    public float arenaBottom =  -6f;

    [Tooltip("How far inset from the wall edge the balloon spawns")]
    public float wallOffset = 0.5f;

    private const float MaxRespawnDelay = 2f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Clear any manually placed balloons in the scene
        try
        {
            foreach (var b in GameObject.FindGameObjectsWithTag("Balloon"))
                Destroy(b);
        }
        catch (UnityEngine.UnityException) { }

        // Spawn exactly one balloon per wall (4 total) at game start
        for (int wall = 0; wall < 4; wall++)
            SpawnBalloon(wall);
    }

    /// <summary>
    /// Called by BalloonPop when a balloon is popped.
    /// Respawns a new balloon on the same wall within 2 seconds.
    /// </summary>
    public void ScheduleRespawn(int wallIndex)
    {
        if (wallIndex < 0) return;
        StartCoroutine(RespawnAfterDelay(wallIndex));
    }

    private IEnumerator RespawnAfterDelay(int wallIndex)
    {
        yield return new WaitForSeconds(Random.Range(0.5f, MaxRespawnDelay));
        SpawnBalloon(wallIndex);
    }

    private void SpawnBalloon(int wallIndex)
    {
        if (balloonPrefab == null) return;

        Vector3 pos = GetRandomWallPosition(wallIndex);
        GameObject balloon = Instantiate(balloonPrefab, pos, Quaternion.identity);
        balloon.tag = "Balloon";

        BalloonPop pop = balloon.GetComponent<BalloonPop>();
        if (pop != null) pop.wallIndex = wallIndex;
    }

    /// <summary>
    /// Returns a random position along the given wall edge.
    /// 0 = top, 1 = bottom, 2 = left, 3 = right.
    /// </summary>
    private Vector3 GetRandomWallPosition(int wallIndex)
    {
        switch (wallIndex)
        {
            case 0: // top
                return new Vector3(Random.Range(arenaLeft + 1f, arenaRight - 1f), arenaTop - wallOffset, 0f);
            case 1: // bottom
                return new Vector3(Random.Range(arenaLeft + 1f, arenaRight - 1f), arenaBottom + wallOffset, 0f);
            case 2: // left
                return new Vector3(arenaLeft + wallOffset, Random.Range(arenaBottom + 1f, arenaTop - 1f), 0f);
            case 3: // right
            default:
                return new Vector3(arenaRight - wallOffset, Random.Range(arenaBottom + 1f, arenaTop - 1f), 0f);
        }
    }
}
