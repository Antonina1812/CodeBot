using UnityEngine;

[DefaultExecutionOrder(-200)]
public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private GameData _defaultData;

    private GameData _runtimeData;

    public bool HasSave { get; private set; }
    public float MusicVolume => _runtimeData.musicVolume;
    public float SfxVolume => _runtimeData.sfxVolume;
    public int LastUnlockedLevel => _runtimeData.lastUnlockedLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_defaultData == null)
        {
            enabled = false;
            return;
        }

        _runtimeData = ScriptableObject.CreateInstance<GameData>();
        _runtimeData.musicVolume = _defaultData.musicVolume;
        _runtimeData.sfxVolume = _defaultData.sfxVolume;
        _runtimeData.lastUnlockedLevel = _defaultData.lastUnlockedLevel;

        HasSave = SaveSystem.Load(_runtimeData);
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    public void Save()
    {
        if (_runtimeData == null) return;
        SaveSystem.Save(_runtimeData);
    }

    public void SetMusicVolume(float value)
    {
        _runtimeData.musicVolume = value;
    }

    public void SetSfxVolume(float value)
    {
        _runtimeData.sfxVolume = value;
    }

    public void UnlockLevel(int levelIndex)
    {
        if (levelIndex <= _runtimeData.lastUnlockedLevel) return;
        _runtimeData.lastUnlockedLevel = levelIndex;
        Save();
    }

    public void ResetProgress()
    {
        if (_runtimeData == null) return;

        float savedMusic = _runtimeData.musicVolume;
        float savedSfx = _runtimeData.sfxVolume;

        _runtimeData.lastUnlockedLevel = 1;
        HasSave = false;

        _runtimeData.musicVolume = savedMusic;
        _runtimeData.sfxVolume = savedSfx;

        SaveSystem.DeleteSave();
        Save();
    }
}