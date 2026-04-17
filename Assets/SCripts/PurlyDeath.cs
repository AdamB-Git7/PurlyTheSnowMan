using UnityEngine;

/// <summary>
/// Listens for the GameOver event and hides Purly's GameObject when he dies.
/// Attach to the Purly root GameObject.
/// </summary>
public class PurlyDeath : MonoBehaviour
{
    void OnEnable()
    {
        // Subscribe when this object becomes active so it reacts to game over.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += HandleDeath;
        }
    }

    void OnDisable()
    {
        // Unsubscribe when disabled so the handler is not left behind.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= HandleDeath;
        }
    }

    private void HandleDeath()
    {
        // Hide the player object instead of destroying it outright.
        gameObject.SetActive(false);
    }
}
