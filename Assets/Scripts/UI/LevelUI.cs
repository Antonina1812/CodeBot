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

    [Header("Статистика")]
    [SerializeField] private GameObject _statsPanel;              // Панель со статистикой
    [SerializeField] private TextMeshProUGUI _commandsUsedText;   // "Команд использовано: X"
    [SerializeField] private TextMeshProUGUI _idealCommandsText;  // "Идеальное количество: Y"
    [SerializeField] private TextMeshProUGUI _efficiencyText;     // "Эффективность: 85%"
    [SerializeField] private Image _efficiencyFillImage;          // Заполняемая полоска эффективности
    [SerializeField] private int _idealCommandsCount = 5;         // Идеальное количество команд для уровня

    private float _levelStartTime;
    private int _commandsExecuted = 0;  // Счетчик выполненных команд

    void Awake()
    {
        Instance = this;
        _resultPanel?.SetActive(false);
        _settingsPanel?.SetActive(false);
        _statsPanel?.SetActive(false);
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

        SetupAttemptsForLevel();
        UpdateAttemptsDisplay();
    }

    private void SetupAttemptsForLevel()
    {
        if (GameProgress.Instance == null) return;

        int currentLevelIndex = GetCurrentLevelIndex();
        string currentLevelName = SceneManager.GetActiveScene().name;
        
        string lastLevelKey = "LastPlayedLevel";
        string lastAttemptsKey = "CurrentAttempts";
        
        string lastLevel = PlayerPrefs.GetString(lastLevelKey, "");
        bool isNewLevel = (lastLevel != currentLevelName);
        bool hasAttemptsForThisLevel = PlayerPrefs.HasKey(lastAttemptsKey) && !isNewLevel;
        
        if (isNewLevel)
        {
            GameProgress.Instance.SetAttemptsForLevel(currentLevelIndex, _attemptsForThisLevel);
            PlayerPrefs.SetInt(lastAttemptsKey, _attemptsForThisLevel);
            Debug.Log($"Новый уровень {currentLevelName}! Установлено {_attemptsForThisLevel} попыток");
        }
        else if (hasAttemptsForThisLevel)
        {
            int savedAttempts = PlayerPrefs.GetInt(lastAttemptsKey);
            GameProgress.Instance.SetAttemptsForLevel(currentLevelIndex, savedAttempts);
            Debug.Log($"Перезагрузка уровня {currentLevelName}. Восстановлено {savedAttempts} попыток");
        }
        else
        {
            GameProgress.Instance.SetAttemptsForLevel(currentLevelIndex, _attemptsForThisLevel);
            PlayerPrefs.SetInt(lastAttemptsKey, _attemptsForThisLevel);
            Debug.Log($"Первый запуск уровня {currentLevelName}. Установлено {_attemptsForThisLevel} попыток");
        }
        
        PlayerPrefs.SetString(lastLevelKey, currentLevelName);
        PlayerPrefs.Save();
    }

    void UpdateAttemptsDisplay()
    {
        if (_attemptsText != null && GameProgress.Instance != null)
        {
            int attempts = GameProgress.Instance.AttemptsLeft;
            _attemptsText.text = $"Попытки: {attempts}";
            
            PlayerPrefs.SetInt("CurrentAttempts", attempts);
            PlayerPrefs.Save();
            
            if (attempts <= 1)
                _attemptsText.color = Color.red;
            else if (attempts <= 2)
                _attemptsText.color = Color.yellow;
            else
                _attemptsText.color = Color.white;
        }
    }

    // Метод для подсчета команд (вызывается из CodeExecutor)
    public void AddExecutedCommand()
    {
        _commandsExecuted++;
    }

    // Метод для сброса счетчика команд (вызывается перед выполнением)
    public void ResetCommandCounter()
    {
        _commandsExecuted = 0;
    }

    public void ShowWin()
    {
        int currentLevel = GetCurrentLevelIndex();
        GameProgress.Instance?.UnlockLevel(currentLevel + 1);
        GameProgress.Instance?.Save();

        PlayerPrefs.DeleteKey("CurrentAttempts");
        
        _titleText.text = "Уровень пройден!";
        _timeText.text = GetTimeString();
        
        // Показываем статистику
        ShowStatistics();
        
        _nextLevelButton?.gameObject.SetActive(true);
        _resultPanel?.SetActive(true);
        
        Debug.Log($"Победа! Разблокирован уровень {currentLevel + 1}");
    }

    private void ShowStatistics()
    {
        if (_statsPanel == null) return;
        
        _statsPanel.SetActive(true);
        
        // Отображаем количество использованных команд
        if (_commandsUsedText != null)
            _commandsUsedText.text = $"Команд использовано: {_commandsExecuted}";
        
        // Отображаем идеальное количество команд
        if (_idealCommandsText != null)
            _idealCommandsText.text = $"Идеальное количество: {_idealCommandsCount}";
        
        // Вычисляем эффективность
        float efficiency = CalculateEfficiency();
        
        // Отображаем процент эффективности
        if (_efficiencyText != null)
        {
            _efficiencyText.text = $"Эффективность: {efficiency:F1}%";
            
            // Цвет в зависимости от эффективности
            if (efficiency >= 90f)
                _efficiencyText.color = Color.green;
            else if (efficiency >= 70f)
                _efficiencyText.color = Color.yellow;
            else
                _efficiencyText.color = Color.red;
        }
        
        // Заполняем полоску эффективности
        if (_efficiencyFillImage != null)
        {
            _efficiencyFillImage.fillAmount = efficiency / 100f;
            
            // Цвет полоски
            if (efficiency >= 75f)
                _efficiencyFillImage.color = Color.green;
            else if (efficiency >= 50f)
                _efficiencyFillImage.color = Color.yellow;
            else
                _efficiencyFillImage.color = Color.red;
        }
        
        // Сохраняем лучший результат для уровня
        SaveBestEfficiency(efficiency);
    }

    private float CalculateEfficiency()
    {
        if (_idealCommandsCount <= 0) return 100f;
        if (_commandsExecuted <= 0) return 0f;
        
        // Если использовано меньше или равно идеальному - 100%
        if (_commandsExecuted <= _idealCommandsCount)
            return 100f;
        
        // Иначе вычисляем процент
        float efficiency = ((float)_idealCommandsCount / _commandsExecuted) * 100f;
        return Mathf.Clamp(efficiency, 0f, 100f);
    }

    private void SaveBestEfficiency(float efficiency)
    {
        string key = $"BestEfficiency_Level{GetCurrentLevelIndex()}";
        float bestEfficiency = PlayerPrefs.GetFloat(key, 0f);
        
        if (efficiency > bestEfficiency)
        {
            PlayerPrefs.SetFloat(key, efficiency);
            PlayerPrefs.Save();
            Debug.Log($"Новый рекорд эффективности для уровня {GetCurrentLevelIndex()}: {efficiency:F1}%");
        }
    }

    public void ShowLose()
    {
        if (GameProgress.Instance == null) return;

        GameProgress.Instance.DecreaseAttempt();
        UpdateAttemptsDisplay();

        int attemptsLeft = GameProgress.Instance.AttemptsLeft;
        
        // Скрываем статистику при проигрыше
        if (_statsPanel != null)
            _statsPanel.SetActive(false);
        
        if (attemptsLeft <= 0)
        {
            _titleText.text = "Попытки закончились!";
            _timeText.text = "Прогресс сброшен до 1 уровня";
            _nextLevelButton?.gameObject.SetActive(false);
            _retryButton?.gameObject.SetActive(false);
            
            Debug.Log("Все попытки потрачены! Прогресс сброшен.");
        }
        else
        {
            _titleText.text = "Попробуй ещё раз!";
            _timeText.text = GetTimeString();
            _nextLevelButton?.gameObject.SetActive(false);
            _retryButton?.gameObject.SetActive(true);
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
        
        PlayerPrefs.DeleteKey("CurrentAttempts");
        PlayerPrefs.SetString("LastPlayedLevel", nextLevelName);
        PlayerPrefs.Save();
        
        SceneManager.LoadScene(nextLevelName);
    }

    private void ReloadLevel()
    {
        if (GameProgress.Instance != null && GameProgress.Instance.AttemptsLeft <= 0)
        {
            Debug.Log("Попытки закончились! Возврат в меню.");
            SceneManager.LoadScene("MainMenu");
            return;
        }
        
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