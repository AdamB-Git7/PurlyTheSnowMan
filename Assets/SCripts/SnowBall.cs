using UnityEngine;

/// <summary>
/// Moves in a straight line in the direction set by SnowGun.
/// Minimum speed is 6.5f. Destroys itself after 10 seconds (lifetime) or
/// on hitting the player. A short grace period prevents immediate self-destruction
/// against the spawning wall.
/// </summary>
public class Snowball : MonoBehaviour
{
    public float speed = 6.5f;

    private const float MinSpeed = 6.5f;
    private const float LifeTime = 10f;

    // Seconds after spawn during which wall collisions are ignored
    private const float CollisionGracePeriod = 0.5f;

    private Vector2 direction = Vector2.left;
    private float age = 0f;

    void Start()
    {
        speed = Mathf.Max(speed, MinSpeed);
        Destroy(gameObject, LifeTime);
    }

    /// <summary>Sets the travel direction. Called by SnowGun immediately after instantiation.</summary>
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        age += Time.deltaTime;
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.Instance?.OnPurlyDied();
            Destroy(gameObject);
        }
        else if (age >= CollisionGracePeriod)
        {
            // Hit a wall after clearing the spawn side — despawn
            Destroy(gameObject);
        }
    }
}