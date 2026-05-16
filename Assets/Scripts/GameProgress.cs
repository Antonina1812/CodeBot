using UnityEngine;

[DefaultExecutionOrder(-200)]
public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private GameData _defaultData;
    [SerializeField] private int _defaultAttempts = 3;

    private GameData _runtimeData;

    public bool HasSave { get; private set; }
    public float MusicVolume => _runtimeData.musicVolume;
    public float SfxVolume => _runtimeData.sfxVolume;
    public int LastUnlockedLevel => _runtimeData.lastUnlockedLevel;
    public int AttemptsLeft => _runtimeData.attemptsLeft;

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
            Debug.LogError("GameProgress: defaultData не назначен!");
            enabled = false;
            return;
        }

        _runtimeData = ScriptableObject.CreateInstance<GameData>();
        _runtimeData.musicVolume = _defaultData.musicVolume;
        _runtimeData.sfxVolume = _defaultData.sfxVolume;
        _runtimeData.lastUnlockedLevel = _defaultData.lastUnlockedLevel;
        _runtimeData.attemptsLeft = _defaultData.attemptsLeft;

        // Пытаемся загрузить сохранение
        HasSave = SaveSystem.Load(_runtimeData);
        
        if (HasSave)
        {
            Debug.Log($"Загружено сохранение. Попытки: {_runtimeData.attemptsLeft}");
        }
        else
        {
            // Первый запуск - используем значения по умолчанию
            _runtimeData.attemptsLeft = _defaultAttempts;
            Debug.Log($"Новая игра. Попытки: {_defaultAttempts}");
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    private void OnDestroy()
    {
        Save();
    }

    public void Save()
    {
        if (_runtimeData == null) return;
        SaveSystem.Save(_runtimeData);
        Debug.Log($"Прогресс сохранен. Попытки: {_runtimeData.attemptsLeft}");
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
        Debug.Log($"Разблокирован уровень {levelIndex}");
    }

    public void SetAttemptsForLevel(int levelIndex, int attempts)
    {
        _runtimeData.attemptsLeft = attempts;
        Save();
        Debug.Log($"Установлено {attempts} попыток для уровня {levelIndex}");
    }

    public void DecreaseAttempt()
    {
        _runtimeData.attemptsLeft--;
        Debug.Log($"Потрачена попытка. Осталось: {_runtimeData.attemptsLeft}");
        
        if (_runtimeData.attemptsLeft <= 0)
        {
            ResetProgress();
        }
        else
        {
            Save();
        }
    }

    public void ResetProgress()
    {
        Debug.Log("Сброс прогресса до 1 уровня!");
        _runtimeData.lastUnlockedLevel = 1;
        Save();
    }
}   