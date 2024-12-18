using UnityEngine;

public class GhostBehavior : MonoBehaviour
{
    public GameObject ghostPrefab; // The ghost GameObject
    public Light pointLight; // The point light to activate and deactivate
    public Transform[] positions; // Array of predefined positions
    public float speed = 2f; // Speed of movement

    public AudioSource audioSource1; // First audio source
    public AudioSource audioSource2; // Second audio source
    public float audioPlayChance = 0.5f; // Chance to play an audio clip (0 to 1)

    private int currentPositionIndex = 0; // Index of the current position
    private bool isMoving = false; // Flag to track movement

    private void Update()
    {
        if (isMoving)
        {
            // Gradually move the ghost toward the next position
            MoveToPosition();

            // Play audio with a random chance if it's not already playing
            if (!audioSource1.isPlaying && !audioSource2.isPlaying)
            {
                TryPlayRandomAudio();
            }
        }
    }

    /// <summary>
    /// Activates the ghost and starts moving it to the next position.
    /// </summary>
    public void MoveToNextPosition()
    {
        if (positions.Length == 0) return;

        // Activate the ghost and point light
        ghostPrefab.SetActive(true);
        pointLight.enabled = true;

        // Start moving toward the next position
        isMoving = true;
    }

    /// <summary>
    /// Moves the ghost to the next predefined position.
    /// </summary>
    private void MoveToPosition()
    {
        // Determine the target position
        Transform target = positions[currentPositionIndex];

        // Calculate the step size for this frame
        float step = speed * Time.deltaTime;

        // Move the ghost toward the target position
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);

        // Check if the ghost has reached the target position
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            // Snap to the target position
            transform.position = target.position;

            // Deactivate the ghost and point light
            ghostPrefab.SetActive(false);
            pointLight.enabled = false;

            // Update to the next position in the sequence
            currentPositionIndex = (currentPositionIndex + 1) % positions.Length;

            // Stop moving
            isMoving = false;
        }
    }


    /// <summary>
    /// Randomly decides whether to play one of the audio sources.
    /// </summary>
    private void TryPlayRandomAudio()
    {
        if (Random.value < audioPlayChance)
        {
            // Randomly select one of the audio sources
            AudioSource selectedAudio = Random.value < 0.5f ? audioSource1 : audioSource2;
            selectedAudio.Play();
        }
    }
}