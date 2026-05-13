using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelUI : MonoBehaviour
{
    public static LevelUI Instance { get; private set; }

    [Header("Панель результата")]
    [SerializeField] private GameObject _resultPanel;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private Button _nextLevelButton;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _resultMenuButton;

    [Header("Боковые кнопки")]
    [SerializeField] private Button _menuButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;

    [Header("Панель настроек")]
    [SerializeField] private GameObject _settingsPanel;

    [Header("Попытки")]
    [SerializeField] private TextMeshProUGUI _attemptsText;
    [SerializeField] private int _attemptsForThisLevel = 3;

    private float _levelStartTime;
    private bool _isFirstLoad = true;

    void Awake()
    {
        Instance = this;
        _resultPanel?.SetActive(false);
        _settingsPanel?.SetActive(false);
        _levelStartTime = Time.time;
    }

    void Start()
{
    _retryButton?.onClick.AddListener(ReloadLevel);
    _resultMenuButton?.onClick.AddListener(GoToMenu);
    _nextLevelButton?.onClick.AddListener(LoadNextLevel);
    _menuButton?.onClick.AddListener(GoToMenu);
    _settingsButton?.onClick.AddListener(ToggleSettings);
    _quitButton?.onClick.AddListener(QuitGame);

    // Устанавливаем попытки при первом заходе на уровень
    SetupAttemptsForLevel();
    
    UpdateAttemptsDisplay();
}

private void SetupAttemptsForLevel()
{
    if (GameProgress.Instance == null) return;

    int currentLevelIndex = GetCurrentLevelIndex();
    string currentLevelName = SceneManager.GetActiveScene().name;
    
    // Ключ для хранения информации о последнем уровне
    string lastLevelKey = "LastPlayedLevel";
    string lastAttemptsKey = "CurrentAttempts";
    
    // Получаем сохраненный последний уровень
    string lastLevel = PlayerPrefs.GetString(lastLevelKey, "");
    
    // Проверяем, новый ли это уровень
    bool isNewLevel = (lastLevel != currentLevelName);
    
    // Проверяем, есть ли уже попытки в сохранении для этого уровня
    bool hasAttemptsForThisLevel = PlayerPrefs.HasKey(lastAttemptsKey) && !isNewLevel;
    
    if (isNewLevel)
    {
        // Это новый уровень - устанавливаем начальные попытки
        GameProgress.Instance.SetAttemptsForLevel(currentLevelIndex, _attemptsForThisLevel);
        PlayerPrefs.SetInt(lastAttemptsKey, _attemptsForThisLevel);
        Debug.Log($"Новый уровень {currentLevelName}! Установлено {_attemptsForThisLevel} попыток");
    }
    else if (hasAttemptsForThisLevel)
    {
        // Это перезагрузка того же уровня - восстанавливаем сохраненные попытки
        int savedAttempts = PlayerPrefs.GetInt(lastAttemptsKey);
        GameProgress.Instance.SetAttemptsForLevel(currentLevelIndex, savedAttempts);
        Debug.Log($"Перезагрузка уровня {currentLevelName}. Восстановлено {savedAttempts} попыток");
    }
    else
    {
        // Первый заход в игре или что-то пошло не так
        GameProgress.Instance.SetAttemptsForLevel(currentLevelIndex, _attemptsForThisLevel);
        PlayerPrefs.SetInt(lastAttemptsKey, _attemptsForThisLevel);
        Debug.Log($"Первый запуск уровня {currentLevelName}. Установлено {_attemptsForThisLevel} попыток");
    }
    
    // Сохраняем текущий уровень как последний
    PlayerPrefs.SetString(lastLevelKey, currentLevelName);
    PlayerPrefs.Save();
}

void UpdateAttemptsDisplay()
{
    if (_attemptsText != null && GameProgress.Instance != null)
    {
        int attempts = GameProgress.Instance.AttemptsLeft;
        _attemptsText.text = $"Попытки: {attempts}";
        
        // Сохраняем текущие попытки в PlayerPrefs
        PlayerPrefs.SetInt("CurrentAttempts", attempts);
        PlayerPrefs.Save();
        
        // Цветовое предупреждение
        if (attempts <= 1)
            _attemptsText.color = Color.red;
        else if (attempts <= 2)
            _attemptsText.color = Color.yellow;
        else
            _attemptsText.color = Color.white;
    }
}

    public void ShowWin()
{
    int currentLevel = GetCurrentLevelIndex();
    GameProgress.Instance?.UnlockLevel(currentLevel + 1);
    GameProgress.Instance?.Save();

    // Очищаем сохраненные попытки для следующего уровня
    PlayerPrefs.DeleteKey("CurrentAttempts");
    
    _titleText.text = "Уровень пройден!";
    _timeText.text = GetTimeString();
    _nextLevelButton?.gameObject.SetActive(true);
    _resultPanel?.SetActive(true);
    
    Debug.Log($"Победа! Разблокирован уровень {currentLevel + 1}");
}

    public void ShowLose()
    {
        if (GameProgress.Instance == null) return;

        // Уменьшаем попытки
        GameProgress.Instance.DecreaseAttempt();
        UpdateAttemptsDisplay();

        int attemptsLeft = GameProgress.Instance.AttemptsLeft;
        Debug.Log("QWERTY - " + attemptsLeft);
        if (attemptsLeft <= 0)
        {
            _titleText.text = "Попытки закончились!";
            _timeText.text = "Прогресс сброшен до 1 уровня";
            _nextLevelButton?.gameObject.SetActive(false);
            _retryButton?.gameObject.SetActive(false); // Блокируем Retry
            
            Debug.Log("Все попытки потрачены! Прогресс сброшен.");
        }
        else
        {
            _titleText.text = "Попробуй ещё раз!";
            _timeText.text = GetTimeString();
            _nextLevelButton?.gameObject.SetActive(false);
            _retryButton?.gameObject.SetActive(true); // Retry доступен
        }
        
        _resultPanel?.SetActive(true);
    }

    public void ToggleSettings()
    {
        if (_settingsPanel == null) return;
        _settingsPanel.SetActive(!_settingsPanel.activeSelf);
    }

    public void GoToMenu()
{
    Time.timeScale = 1f;
    
    // Если попытки закончились - полностью сбрасываем
    if (GameProgress.Instance != null && GameProgress.Instance.AttemptsLeft <= 0)
    {
        PlayerPrefs.DeleteKey("LastPlayedLevel");
        PlayerPrefs.DeleteKey("CurrentAttempts");
        PlayerPrefs.Save();
        Debug.Log("Попытки закончились! Сброс прогресса.");
    }
    
    SceneManager.LoadScene("MainMenu");
}

    public void QuitGame()
    {
        Application.Quit();
    }

    private void LoadNextLevel()
{
    int next = GetCurrentLevelIndex() + 1;
    string nextLevelName = "Level" + next;
    
    // Очищаем данные о попытках для нового уровня
    PlayerPrefs.DeleteKey("CurrentAttempts");
    PlayerPrefs.SetString("LastPlayedLevel", nextLevelName);
    PlayerPrefs.Save();
    
    SceneManager.LoadScene(nextLevelName);
}

    private void ReloadLevel()
    {
        // Проверяем, не закончились ли попытки
        if (GameProgress.Instance != null && GameProgress.Instance.AttemptsLeft <= 0)
        {
            Debug.Log("Попытки закончились! Возврат в меню.");
            SceneManager.LoadScene("MainMenu");
            return;
        }
        
        // Обычная перезагрузка уровня
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private int GetCurrentLevelIndex()
    {
        string name = SceneManager.GetActiveScene().name;
        if (int.TryParse(name.Replace("Level", ""), out int idx))
            return idx;
        return 1;
    }

    private string GetTimeString()
    {
        float elapsed = Time.time - _levelStartTime;
        int minutes = Mathf.FloorToInt(elapsed / 60);
        int seconds = Mathf.FloorToInt(elapsed % 60);
        return $"Время: {minutes:00}:{seconds:00}";
    }
}