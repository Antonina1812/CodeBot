using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    private bool _subscribed;

    private void OnEnable()
    {
        if (GameProgress.Instance == null)
        {
            return;
        }

        _musicSlider.SetValueWithoutNotify(GameProgress.Instance.MusicVolume);
        _sfxSlider.SetValueWithoutNotify(GameProgress.Instance.SfxVolume);

        if (!_subscribed)
        {
            _musicSlider.onValueChanged.AddListener(OnMusicChanged);
            _sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            _subscribed = true;
        }
    }

    private void OnDisable()
    {
        if (_subscribed)
        {
            _musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
            _sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
            _subscribed = false;
        }

        GameProgress.Instance?.Save();
    }

    private void OnMusicChanged(float value)
    {
        GameProgress.Instance.SetMusicVolume(value);
        AudioManager.Instance?.ApplyVolume();
    }

    private void OnSfxChanged(float value)
    {
        GameProgress.Instance.SetSfxVolume(value);
        AudioManager.Instance?.ApplyVolume();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}