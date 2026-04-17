using UnityEngine;

/// <summary>
/// Detects when Purly touches a balloon. Awards 1 point and triggers
/// respawn on the same wall via BalloonManager within 2 seconds.
/// </summary>
public class BalloonPop : MonoBehaviour
{
    // Wall index (0=top, 1=bottom, 2=left, 3=right) — set by BalloonManager at spawn time
    [HideInInspector] public int wallIndex = -1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance?.AddScore(1);
            BalloonManager.Instance?.ScheduleRespawn(wallIndex);
            Destroy(gameObject);
        }
    }
}