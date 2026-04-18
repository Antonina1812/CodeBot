using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public Image fadeImage;

    public float fadeSpeed = 2f;

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        StartCoroutine(FadeIn());
        Time.timeScale = 0f;
        
    }

    public void ResumeGame()
    {
        StartCoroutine(FadeOut());
        Time.timeScale = 1f;
        
    }

    IEnumerator FadeIn()
    {
        Color c = fadeImage.color;

        while (c.a < 0.6f)
        {
            c.a += Time.unscaledDeltaTime * fadeSpeed;
            fadeImage.color = c;
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        Color c = fadeImage.color;

        while (c.a > 0)
        {
            c.a -= Time.unscaledDeltaTime * fadeSpeed;
            fadeImage.color = c;
            yield return null;
        }

        pauseMenu.SetActive(false);
    }
}
