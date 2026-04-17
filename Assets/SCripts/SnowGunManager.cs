using UnityEngine;

/// <summary>
/// Coordinates two snow guns so they fire strictly one at a time, alternating
/// with a 20-second gap between each activation, never simultaneously.
/// The first shot fires within the first 5 seconds (random 1-5s initial delay),
/// then every 20 seconds the next gun fires, alternating indefinitely.
/// </summary>
public class SnowGunManager : MonoBehaviour
{
    [Header("Snow Guns (assign Gun 0 and Gun 1)")]
    // First gun in the alternating sequence.
    public SnowGun gun0;

    // Second gun in the alternating sequence.
    public SnowGun gun1;

    [Header("Timing")]
    [Tooltip("Gap in seconds between one gun firing and the next gun firing.")]
    // Delay between every shot after the first one.
    public float gapBetweenGuns = 20f;

    [Tooltip("Max seconds before the very first shot. Actual delay is random between 1 and this value.")]
    // Upper limit for the first random delay.
    public float maxInitialDelay = 5f;

    // Tracks which gun should fire next.
    private int currentGunIndex = 0;

    // Accumulates elapsed time toward the next shot.
    private float timer = 0f;

    // Current time target that must be reached before firing.
    private float currentThreshold;

    void Start()
    {
        // Delay the first shot by a short random amount instead of firing immediately.
        currentThreshold = Random.Range(1f, maxInitialDelay);
    }

    void Update()
    {
        // Do not advance the firing timer unless the game is actively running.
        if (!IsGamePlaying())
        {
            return;
        }

        // Accumulate time until the next scheduled shot.
        timer += Time.deltaTime;

        // Wait until the threshold is reached before firing.
        if (timer < currentThreshold)
        {
            return;
        }

        // Reset the timer for the next interval.
        timer = 0f;

        // After the first shot, always use the regular gun gap.
        currentThreshold = gapBetweenGuns;

        // Fire the active gun.
        FireCurrentGun();

        // Alternate to the other gun for the next firing cycle.
        currentGunIndex = (currentGunIndex + 1) % 2;
    }

    /// <summary>Fires whichever gun is currently active in the alternation sequence.</summary>
    private void FireCurrentGun()
    {
        // Select the correct gun based on the current alternation state.
        SnowGun activeGun = currentGunIndex == 0 ? gun0 : gun1;

        // Fire only if that gun reference exists.
        activeGun?.Shoot();
    }

    private bool IsGamePlaying()
    {
        // The manager should only operate while the GameManager says gameplay is active.
        return GameManager.Instance != null && GameManager.Instance.IsPlaying;
    }
}
