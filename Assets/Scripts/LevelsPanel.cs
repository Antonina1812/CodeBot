using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelsPanel : MonoBehaviour
{
    [SerializeField] private Button[] _levelButtons;

    private void OnEnable()
    {
        if (GameProgress.Instance == null) return;

        int lastUnlocked = GameProgress.Instance.LastUnlockedLevel;

        for (int i = 0; i < _levelButtons.Length; i++)
        {
            if (_levelButtons[i] == null) continue;
            _levelButtons[i].interactable = (i + 1) <= lastUnlocked;
        }
    }

    public void LoadLevel(int levelIndex)
    {
        SceneManager.LoadScene("Level" + levelIndex);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}