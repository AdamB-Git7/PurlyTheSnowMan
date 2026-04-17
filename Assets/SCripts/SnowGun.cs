using UnityEngine;

/// <summary>
/// Represents a single snow gun. Fires a snowball toward the opposite wall
/// in a straight, random direction. Called externally by SnowGunManager.
/// </summary>
public class SnowGun : MonoBehaviour
{
    [Header("Snowball")]
    // Projectile prefab this gun spawns.
    public GameObject snowballPrefab;

    [Tooltip("Units to offset the spawn point inward from the gun so the ball clears the wall collider.")]
    // Distance moved inward from the wall before the projectile appears.
    public float spawnInwardOffset = 2f;

    /// <summary>
    /// Fires one snowball toward the opposite wall in a random straight direction.
    /// Called by SnowGunManager to enforce strict alternation with a 20-second gap.
    /// </summary>
    public void Shoot()
    {
        // If no projectile prefab was assigned, there is nothing to fire.
        if (snowballPrefab == null)
        {
            return;
        }

        // Choose left or right based on which side of the arena this gun sits on.
        float horizontalDir = transform.position.x > 0f ? -1f : 1f;

        // Add a slight random vertical angle so shots are not perfectly flat every time.
        float randomVertical = Random.Range(-0.3f, 0.3f);
        Vector2 direction = new Vector2(horizontalDir, randomVertical).normalized;

        // Spawn the projectile slightly inside the arena to avoid instant wall collisions.
        Vector3 spawnPos = transform.position + new Vector3(horizontalDir * spawnInwardOffset, 0f, 0f);

        // Create the projectile.
        GameObject ball = Instantiate(snowballPrefab, spawnPos, Quaternion.identity);

        // Read the Snowball component so its movement direction can be set.
        Snowball snowball = ball.GetComponent<Snowball>();

        // If the prefab is missing the Snowball script, destroy the bad instance immediately.
        if (snowball == null)
        {
            Destroy(ball);
            return;
        }

        // Pass the chosen direction to the projectile.
        snowball.SetDirection(direction);
    }
}
