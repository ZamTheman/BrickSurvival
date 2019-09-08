using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    instance = new GameObject("Spawned Audiomanager", typeof(AudioManager)).GetComponent<AudioManager>();
                }
            }

            return instance;
        }

        private set
        {
            instance = value;
        }
    }

    private AudioSource sfxSource;
    private AudioSource bossSource;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        sfxSource = gameObject.AddComponent<AudioSource>();
        bossSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayEffect(AudioClip audioClip)
    {
        sfxSource.PlayOneShot(audioClip);
    }

    public void PlayEffect(AudioClip audioClip, float volume)
    {
        sfxSource.PlayOneShot(audioClip, volume);
    }

    public void PlayBossEffect(AudioClip audioClip, float volume)
    {
        bossSource.PlayOneShot(audioClip, volume);
    }
}
