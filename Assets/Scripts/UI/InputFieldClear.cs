using UnityEngine;
using TMPro;

public class InputFieldClear : MonoBehaviour
{
    private TMP_InputField _input;

    void Start()
    {
        _input = GetComponent<TMP_InputField>();
    }

    void Update()
    {
        if (_input == null) return;

        if (_input.isFocused)
            _input.placeholder.gameObject.SetActive(false);
        else if (string.IsNullOrEmpty(_input.text))
            _input.placeholder.gameObject.SetActive(true);
    }
}