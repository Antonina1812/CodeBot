using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject _levelsPanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private Button _continueButton;
    [SerializeField] private TMP_Text _continueButtonText;

    private bool _hasSave;

    private void Start()
    {
        _levelsPanel.SetActive(false);
        _settingsPanel.SetActive(false);

        _hasSave = GameProgress.Instance != null && GameProgress.Instance.HasSave;
        if (_continueButtonText != null)
            _continueButtonText.text = _hasSave ? "Продолжить" : "Начать";
    }

    public void OnContinueClick()
    {
        if (_hasSave && GameProgress.Instance != null)
        {
            int lvl = Mathf.Max(1, GameProgress.Instance.LastUnlockedLevel);
            SceneManager.LoadScene("Level" + lvl);
        }
        else
        {
            SceneManager.LoadScene("Level1");
        }
    }

    public void OpenLevels()
    {
        _levelsPanel.SetActive(true);
        _settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        _settingsPanel.SetActive(true);
        _levelsPanel.SetActive(false);
    }

    public void CloseAllPanels()
    {
        _levelsPanel.SetActive(false);
        _settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}