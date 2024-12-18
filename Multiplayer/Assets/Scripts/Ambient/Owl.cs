using System.Collections;
using UnityEngine;

public class Owl : MonoBehaviour
{
    public AudioClip owlSound;
    public AudioSource audioSource;
    public float minDelay = 20f;
    public float maxDelay = 50f; 

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = owlSound;
        StartCoroutine(PlayOwlSoundRandomly());
    }

    IEnumerator PlayOwlSoundRandomly()
    {
        while (true)
        {
            // Wait for a random time between minDelay and maxDelay
            float waitTime = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(waitTime);

            // Play the owl sound
            if (owlSound != null && audioSource != null)
            {
                audioSource.Play();
            }
        }
    }
}
