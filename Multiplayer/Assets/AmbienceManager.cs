using UnityEngine;

public class AmbienceManager : MonoBehaviour
{
    public GhostBehavior ghost; // Reference to the GhostBehavior script
    public float minInterval = 5f; // Minimum time before the ghost moves
    public float maxInterval = 10f; // Maximum time before the ghost moves

    public AudioSource grandfatherClockChime; // Audio source for the clock chime
    public float minClockInterval = 30f; // Minimum time before the clock chimes
    public float maxClockInterval = 60f; // Maximum time before the clock chimes

    public AudioSource floorboardCreak; // Audio source for the floorboard creak
    public float minCreakInterval = 15f; // Minimum time before a floorboard creaks
    public float maxCreakInterval = 45f; // Maximum time before a floorboard creaks

    private float ghostTimer; // Countdown timer for triggering ghost movement
    private float clockChimeTimer; // Countdown timer for triggering the clock chime
    private float floorboardCreakTimer; // Countdown timer for triggering a floorboard creak

    private void Start()
    {
        // Initialize timers with random values
        ResetGhostTimer();
        ResetClockChimeTimer();
        ResetFloorboardCreakTimer();
    }

    private void Update()
    {
        // Update the ghost timer
        ghostTimer -= Time.deltaTime;
        if (ghostTimer <= 0f)
        {
            if (ghost != null)
            {
                ghost.MoveToNextPosition();
            }
            ResetGhostTimer();
        }

        // Update the clock chime timer
        clockChimeTimer -= Time.deltaTime;
        if (clockChimeTimer <= 0f)
        {
            PlayClockChime();
            ResetClockChimeTimer();
        }

        // Update the floorboard creak timer
        floorboardCreakTimer -= Time.deltaTime;
        if (floorboardCreakTimer <= 0f)
        {
            PlayFloorboardCreak();
            ResetFloorboardCreakTimer();
        }
    }

    /// <summary>
    /// Resets the ghost movement timer with a random value.
    /// </summary>
    private void ResetGhostTimer()
    {
        ghostTimer = Random.Range(minInterval, maxInterval);
    }

    /// <summary>
    /// Resets the clock chime timer with a random value.
    /// </summary>
    private void ResetClockChimeTimer()
    {
        clockChimeTimer = Random.Range(minClockInterval, maxClockInterval);
    }

    /// <summary>
    /// Resets the floorboard creak timer with a random value.
    /// </summary>
    private void ResetFloorboardCreakTimer()
    {
        floorboardCreakTimer = Random.Range(minCreakInterval, maxCreakInterval);
    }

    /// <summary>
    /// Plays the grandfather clock chime sound.
    /// </summary>
    private void PlayClockChime()
    {
        if (grandfatherClockChime != null)
        {
            grandfatherClockChime.Play();
        }
    }

    /// <summary>
    /// Plays the floorboard creak sound.
    /// </summary>
    private void PlayFloorboardCreak()
    {
        if (floorboardCreak != null)
        {
            floorboardCreak.Play();
        }
    }
}
