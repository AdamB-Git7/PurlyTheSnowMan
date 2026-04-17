using UnityEngine;

/// <summary>
/// Moves in a straight line in the direction set by SnowGun.
/// Minimum speed is 6.5f. Destroys itself after 10 seconds (lifetime) or
/// on hitting the player. A short grace period prevents immediate self-destruction
/// against the spawning wall.
/// </summary>
public class Snowball : MonoBehaviour
{
    // Inspector-exposed speed value for the projectile.
    public float speed = 6.5f;

    // Minimum allowed speed so the projectile always stays dangerous.
    private const float MinSpeed = 6.5f;

    // Hard lifetime limit to clean up old projectiles.
    private const float LifeTime = 10f;

    // Ignore wall collisions briefly so the projectile does not destroy itself on spawn.
    private const float CollisionGracePeriod = 0.5f;

    // Direction the projectile travels in world space.
    private Vector2 direction = Vector2.left;

    // Time since this projectile was spawned.
    private float age = 0f;

    void Start()
    {
        // Clamp the public speed so it cannot drop below the assignment minimum.
        speed = Mathf.Max(speed, MinSpeed);

        // Schedule automatic cleanup even if the projectile hits nothing.
        Destroy(gameObject, LifeTime);
    }

    /// <summary>Sets the travel direction. Called by SnowGun immediately after instantiation.</summary>
    public void SetDirection(Vector2 dir)
    {
        // Normalize so the snowball moves at the same speed regardless of input vector length.
        direction = dir.normalized;
    }

    void Update()
    {
        // Track age for the wall-collision grace-period check.
        age += Time.deltaTime;

        // Move the projectile in a straight line every frame.
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // If the projectile hits the player, end the game and remove the projectile.
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.Instance?.OnPurlyDied();
            Destroy(gameObject);
            return;
        }

        // Once the initial grace period passes, any other collision despawns the snowball.
        if (age >= CollisionGracePeriod)
        {
            Destroy(gameObject);
        }
    }
}
