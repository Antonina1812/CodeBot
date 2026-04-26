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

    private float _levelStartTime;

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
    }

    public void ShowWin()
    {
        int currentLevel = GetCurrentLevelIndex();
        GameProgress.Instance?.UnlockLevel(currentLevel + 1);
        GameProgress.Instance?.Save();

        _titleText.text = "Уровень пройден!";
        _timeText.text = GetTimeString();
        _nextLevelButton?.gameObject.SetActive(true);
        _resultPanel?.SetActive(true);
    }

    public void ShowLose()
    {
        _titleText.text = "Попробуй ещё раз!";
        _timeText.text = GetTimeString();
        _nextLevelButton?.gameObject.SetActive(false);
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
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void LoadNextLevel()
    {
        int next = GetCurrentLevelIndex() + 1;
        SceneManager.LoadScene("Level" + next);
    }

    private void ReloadLevel()
    {
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