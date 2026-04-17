using UnityEngine;

/// <summary>
/// Represents a single snow gun. Fires a snowball toward the opposite wall
/// in a straight, random direction. Called externally by SnowGunManager.
/// </summary>
public class SnowGun : MonoBehaviour
{
    [Header("Snowball")]
    public GameObject snowballPrefab;

    [Tooltip("Units to offset the spawn point inward from the gun so the ball clears the wall collider.")]
    public float spawnInwardOffset = 2f;

    /// <summary>
    /// Fires one snowball toward the opposite wall in a random straight direction.
    /// Called by SnowGunManager to enforce strict alternation with a 20-second gap.
    /// </summary>
    public void Shoot()
    {
        if (snowballPrefab == null) return;

        // Determine horizontal direction toward the opposite wall
        float horizontalDir = transform.position.x > 0f ? -1f : 1f;
        float randomVertical = Random.Range(-0.3f, 0.3f);
        Vector2 direction = new Vector2(horizontalDir, randomVertical).normalized;

        // Spawn inward so the snowball doesn't immediately collide with the wall block
        Vector3 spawnPos = transform.position + new Vector3(horizontalDir * spawnInwardOffset, 0f, 0f);

        GameObject ball = Instantiate(snowballPrefab, spawnPos, Quaternion.identity);
        Snowball snowball = ball.GetComponent<Snowball>();
        if (snowball == null)
        {
            Destroy(ball);
            return;
        }

        snowball.SetDirection(direction);
    }
}