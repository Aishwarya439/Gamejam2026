using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayBGM(string name)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/" + name);
        if (clip == null)
        {
            Debug.LogWarning($"BGM clip not found: Audio/{name}");
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlaySFX(string name)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/" + name);
        if (clip == null)
        {
            Debug.LogWarning($"SFX clip not found: Audio/{name}");
            return;
        }

        sfxSource.PlayOneShot(clip);
    }
}