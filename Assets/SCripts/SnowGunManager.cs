using UnityEngine;

/// <summary>
/// Coordinates two snow guns so they fire strictly one at a time, alternating
/// with a 20-second gap between each activation — never simultaneously.
/// The first shot fires within the first 5 seconds (random 1–5s initial delay),
/// then every 20 seconds the next gun fires, alternating indefinitely.
/// </summary>
public class SnowGunManager : MonoBehaviour
{
    [Header("Snow Guns (assign Gun 0 and Gun 1)")]
    public SnowGun gun0;
    public SnowGun gun1;

    [Header("Timing")]
    [Tooltip("Gap in seconds between one gun firing and the next gun firing.")]
    public float gapBetweenGuns = 20f;

    [Tooltip("Max seconds before the very first shot. Actual delay is random between 1 and this value.")]
    public float maxInitialDelay = 5f;

    private int currentGunIndex = 0;
    private float timer = 0f;
    private float currentThreshold;

    void Start()
    {
        // First shot fires after a short random delay (1 to maxInitialDelay seconds)
        currentThreshold = Random.Range(1f, maxInitialDelay);
    }

    void Update()
    {
        if (!IsGamePlaying()) return;

        timer += Time.deltaTime;

        if (timer >= currentThreshold)
        {
            timer = 0f;
            currentThreshold = gapBetweenGuns; // all subsequent gaps are 20s
            FireCurrentGun();
            currentGunIndex = (currentGunIndex + 1) % 2;
        }
    }

    /// <summary>Fires whichever gun is currently active in the alternation sequence.</summary>
    private void FireCurrentGun()
    {
        SnowGun activeGun = currentGunIndex == 0 ? gun0 : gun1;
        activeGun?.Shoot();
    }

    private bool IsGamePlaying()
    {
        return GameManager.Instance != null && GameManager.Instance.IsPlaying;
    }
}
