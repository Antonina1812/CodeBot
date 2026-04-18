using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer _audioMixer;

    private AudioSource _musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _musicSource = GetComponent<AudioSource>();
        ApplyVolume();
    }

    private void Start()
    {
        ApplyVolume();
    }

    public void ApplyVolume()
    {
        if (_audioMixer == null) return;
        if (GameProgress.Instance == null) return;

        float musicV = GameProgress.Instance.MusicVolume;
        float sfxV = GameProgress.Instance.SfxVolume;

        float musicDb = musicV > 0.001f ? Mathf.Log10(musicV) * 20f : -80f;
        float sfxDb = sfxV > 0.001f ? Mathf.Log10(sfxV) * 20f : -80f;

        _audioMixer.SetFloat("MusicVol", musicDb);
        _audioMixer.SetFloat("SFXVol", sfxDb);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (_musicSource == null || clip == null) return;

        _musicSource.clip = clip;
        _musicSource.Play();
    }

    public void StopMusic()
    {
        if (_musicSource != null)
            _musicSource.Stop();
    }
}