using UnityEngine;

/// <summary>
/// Listens for the GameOver event and hides Purly's GameObject when he dies.
/// Attach to the Purly root GameObject.
/// </summary>
public class PurlyDeath : MonoBehaviour
{
    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += HandleDeath;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= HandleDeath;
    }

    private void HandleDeath()
    {
        gameObject.SetActive(false);
    }
}
