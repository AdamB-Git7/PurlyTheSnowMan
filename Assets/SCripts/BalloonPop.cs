using UnityEngine;

/// <summary>
/// Detects when Purly touches a balloon. Awards 1 point and triggers
/// respawn on the same wall via BalloonManager within 2 seconds.
/// </summary>
public class BalloonPop : MonoBehaviour
{
    // The manager fills this in when the balloon is spawned so the replacement
    // appears on the same wall after the player pops it.
    [HideInInspector] public int wallIndex = -1;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Only the player should be able to pop balloons.
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // Award one point for the pop.
        GameManager.Instance?.AddScore(1);

        // Ask the manager to replace this balloon later on the same wall.
        BalloonManager.Instance?.ScheduleRespawn(wallIndex);

        // Remove the popped balloon from the scene immediately.
        Destroy(gameObject);
    }
}
