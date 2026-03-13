using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CodeEditor : MonoBehaviour
{
    [Header("Спрайты строк кода")]
    [SerializeField] private Sprite _moveForwardSprite;
    [SerializeField] private Sprite _turnLeftSprite;
    [SerializeField] private Sprite _turnRightSprite;
    [SerializeField] private Sprite _collectSprite;

    [Header("Настройки отображения")]
    [SerializeField] private RectTransform _codeLinesContainer;
    [SerializeField] private float _lineSpacing         = 40f;
    [SerializeField] private Vector2 _firstLinePosition = new Vector2(150f, -330f);
    [SerializeField] private Vector2 _lineSize          = new Vector2(150f, 30f);
    [SerializeField] private float _lineScale           = 1f;

    [Header("Кнопки команд")]
    [SerializeField] private Button _moveForwardButton;
    [SerializeField] private Button _turnLeftButton;
    [SerializeField] private Button _turnRightButton;
    [SerializeField] private Button _collectButton;
    [SerializeField] private Button _deleteLastLineButton;

    private List<GameObject> _codeLines = new List<GameObject>();
    private List<string>     _commands  = new List<string>();

    void Start()
    {
        if (_moveForwardButton != null)
            _moveForwardButton.onClick.AddListener(() => AddCodeLine(_moveForwardSprite, "move_forward"));

        if (_turnLeftButton != null)
            _turnLeftButton.onClick.AddListener(() => AddCodeLine(_turnLeftSprite, "turn_left"));

        if (_turnRightButton != null)
            _turnRightButton.onClick.AddListener(() => AddCodeLine(_turnRightSprite, "turn_right"));

        if (_collectButton != null)
            _collectButton.onClick.AddListener(() => AddCodeLine(_collectSprite, "collect"));

        if (_deleteLastLineButton != null)
            _deleteLastLineButton.onClick.AddListener(DeleteLastLine);
    }

    // ── Публичный API ──────────────────────────────────

    public List<string> GetCommands() => new List<string>(_commands);

    public void ClearAll()
    {
        foreach (var line in _codeLines)
            if (line != null) Destroy(line);

        _codeLines.Clear();
        _commands.Clear();
    }

    // ── Внутренняя логика ──────────────────────────────

    void AddCodeLine(Sprite sprite, string commandName)
    {
        if (sprite == null)
        {
            Debug.LogWarning($"Спрайт для {commandName} не назначен!");
            return;
        }
        if (_codeLinesContainer == null)
        {
            Debug.LogError("Контейнер для строк кода не назначен!");
            return;
        }

        float   yPos         = _firstLinePosition.y - (_codeLines.Count * _lineSpacing);
        Vector2 linePosition = new Vector2(_firstLinePosition.x, yPos);

        GameObject newLine = new GameObject($"CodeLine_{commandName}_{_codeLines.Count + 1}");
        newLine.transform.SetParent(_codeLinesContainer, false);

        RectTransform rt    = newLine.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0f, 1f);
        rt.anchorMax        = new Vector2(0f, 1f);
        rt.pivot            = new Vector2(0f, 1f);
        rt.anchoredPosition = linePosition;
        rt.sizeDelta        = _lineSize;
        rt.localScale       = new Vector3(_lineScale, _lineScale, 1f);

        Image img      = newLine.AddComponent<Image>();
        img.sprite         = sprite;
        img.preserveAspect = true;

        _codeLines.Add(newLine);
        _commands.Add(commandName);
    }

    void DeleteLastLine()
    {
        if (_codeLines.Count == 0) return;

        int last = _codeLines.Count - 1;
        if (_codeLines[last] != null) Destroy(_codeLines[last]);
        _codeLines.RemoveAt(last);
        _commands.RemoveAt(last);
    }
}