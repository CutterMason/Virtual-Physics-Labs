using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 1f; // start at full volume
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
            audioSource.volume = Mathf.Clamp01(volume);
    }

    public float GetVolume()
    {
        return audioSource != null ? audioSource.volume : 1f;
    }
}