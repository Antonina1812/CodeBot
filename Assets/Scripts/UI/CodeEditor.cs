using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CodeEditor : MonoBehaviour
{
    [Header("Спрайты")]
    [SerializeField] private Sprite _moveForwardSprite;
    [SerializeField] private Sprite _turnLeftSprite;
    [SerializeField] private Sprite _turnRightSprite;
    [SerializeField] private Sprite _collectSprite;
    [SerializeField] private Sprite _insertSprite;
    [SerializeField] private Sprite _forOpenSprite;
    [SerializeField] private Sprite _forCloseSprite;

    [Header("UI")]
    [SerializeField] private RectTransform _codeLinesContainer;
    [SerializeField] private float   _lineSpacing       = 40f;
    [SerializeField] private Vector2 _firstLinePosition = new Vector2(150f, -330f);
    [SerializeField] private Vector2 _lineSize          = new Vector2(150f, 30f);
    [SerializeField] private float   _lineScale         = 1f;
    [SerializeField] private float   _indentOffset      = 30f;

    [Header("FOR text")]
    [SerializeField] private Color   _forKeywordColor  = new Color(0.4f, 0.7f, 1f);
    [SerializeField] private Color   _forVariableColor = new Color(1f, 0.6f, 0.2f);
    [SerializeField] private Color   _forNumberColor   = new Color(0.5f, 1f, 0.5f);
    [SerializeField] private float   _forLineFontSize  = 14f;
    [SerializeField] private Vector2 _forLineSize      = new Vector2(270f, 30f);

    [Header("Buttons")]
    [SerializeField] private Button _moveForwardButton;
    [SerializeField] private Button _turnLeftButton;
    [SerializeField] private Button _turnRightButton;
    [SerializeField] private Button _collectButton;
    [SerializeField] private Button _insertButton;
    [SerializeField] private Button _forOpenButton;
    [SerializeField] private Button _deleteLastLineButton;

    [Header("Input")]
    [SerializeField] private TMP_InputField _forCountInput;

    private class CodeLineData
    {
        public GameObject obj;
        public int        depth;
        public bool       hidden;
    }

    private List<CodeLineData> _lines    = new List<CodeLineData>();
    private List<string>       _commands = new List<string>();

    private int        _currentDepth = 0;
    private Stack<int> _forStack     = new Stack<int>();

    void Start()
    {
        _moveForwardButton?.onClick.AddListener(() => AddCodeLine(_moveForwardSprite, "move_forward"));
        _turnLeftButton?.onClick.AddListener(()    => AddCodeLine(_turnLeftSprite,    "turn_left"));
        _turnRightButton?.onClick.AddListener(()   => AddCodeLine(_turnRightSprite,   "turn_right"));
        _collectButton?.onClick.AddListener(()     => AddCodeLine(_collectSprite,     "collect"));
        _insertButton?.onClick.AddListener(()      => AddCodeLine(_insertSprite,      "insert"));
        _forOpenButton?.onClick.AddListener(AddForLine);
        _deleteLastLineButton?.onClick.AddListener(DeleteLastLine);
    }

    // ── Публичный API ──────────────────────────────────

    public List<string> GetCommands()
    {
        List<string> result = new List<string>(_commands);
        // Автоматически закрываем незакрытые циклы
        int depth = 0;
        foreach (string cmd in result)
        {
            if (cmd.StartsWith("for:")) depth++;
            else if (cmd == "end_for")  depth--;
        }
        while (depth-- > 0) result.Add("end_for");
        return result;
    }

    public void ClearAll()
    {
        foreach (var line in _lines)
            if (line.obj != null) Destroy(line.obj);
        _lines.Clear();
        _commands.Clear();
        _currentDepth = 0;
        _forStack.Clear();
    }

    // ── FOR ────────────────────────────────────────────

    void AddForLine()
    {
        int count = 1;
        if (_forCountInput != null && !string.IsNullOrEmpty(_forCountInput.text))
            if (!int.TryParse(_forCountInput.text, out count)) count = 1;
        count = Mathf.Max(1, count);

        int depth = _currentDepth;

        GameObject line = new GameObject("for_line");
        line.transform.SetParent(_codeLinesContainer, false);

        var rt = line.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
        rt.sizeDelta  = _forLineSize;
        rt.localScale = new Vector3(_lineScale, _lineScale, 1f);

        var text = line.AddComponent<TextMeshProUGUI>();
        text.richText = true;
        text.fontSize = _forLineFontSize;

        string kw  = ColorUtility.ToHtmlStringRGB(_forKeywordColor);
        string var = ColorUtility.ToHtmlStringRGB(_forVariableColor);
        string num = ColorUtility.ToHtmlStringRGB(_forNumberColor);
        text.text = $"<color=#{kw}>for</color> <color=#{var}>i</color> <color=#{kw}>in range</color>(<color=#{num}>{count}</color>):";

        // Стрелка на строке for
        GameObject arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(line.transform, false);

        var arrowRT = arrowObj.AddComponent<RectTransform>();
        arrowRT.anchorMin = arrowRT.anchorMax = arrowRT.pivot = new Vector2(0f, 0.5f);
        arrowRT.anchoredPosition = new Vector2(-28f, -15f);
        arrowRT.sizeDelta = new Vector2(24f, 24f);

        var arrowImg = arrowObj.AddComponent<Image>();
        arrowImg.sprite = _forOpenSprite;

        var arrowBtn = arrowObj.AddComponent<Button>();

        CodeLineData data = new CodeLineData { obj = line, depth = depth };
        bool isOpen = true;

        arrowBtn.onClick.AddListener(() =>
        {
            isOpen = !isOpen;
            arrowImg.sprite = isOpen ? _forOpenSprite : _forCloseSprite;

            if (!isOpen)
            {
                // Закрываем — добавляем end_for
                int savedDepth = _currentDepth;
                _currentDepth = depth;
                AddCodeLine(_forCloseSprite, "end_for");
                _currentDepth = savedDepth;
                RecalculateDepth();
            }
            else
            {
                // Открываем — убираем последний end_for
                for (int i = _commands.Count - 1; i >= 0; i--)
                {
                    if (_commands[i] == "end_for")
                    {
                        Destroy(_lines[i].obj);
                        _lines.RemoveAt(i);
                        _commands.RemoveAt(i);
                        break;
                    }
                }
                RecalculateDepth();
                RefreshLayout();
            }
        });

        _lines.Add(data);
        _commands.Add($"for:{count}");

        _forStack.Push(_currentDepth);
        _currentDepth++;

        RefreshLayout();
    }

    // ── ОБЫЧНЫЕ КОМАНДЫ ────────────────────────────────

    void AddCodeLine(Sprite sprite, string command)
    {
        if (sprite == null)
        {
            Debug.LogWarning($"Спрайт для {command} не назначен!");
            return;
        }

        GameObject line = new GameObject(command);
        line.transform.SetParent(_codeLinesContainer, false);

        var rt = line.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
        rt.sizeDelta  = _lineSize;
        rt.localScale = new Vector3(_lineScale, _lineScale, 1f);

        var img = line.AddComponent<Image>();
        img.sprite         = sprite;
        img.preserveAspect = true;

        _lines.Add(new CodeLineData { obj = line, depth = _currentDepth });
        _commands.Add(command);

        RefreshLayout();
    }

    // ── УДАЛЕНИЕ ───────────────────────────────────────

    void DeleteLastLine()
    {
        if (_lines.Count == 0) return;

        int last = _lines.Count - 1;
        Destroy(_lines[last].obj);
        _lines.RemoveAt(last);
        _commands.RemoveAt(last);

        RecalculateDepth();
        RefreshLayout();
    }

    void RecalculateDepth()
    {
        _currentDepth = 0;
        _forStack.Clear();

        foreach (string cmd in _commands)
        {
            if (cmd.StartsWith("for:"))
            {
                _forStack.Push(_currentDepth);
                _currentDepth++;
            }
            else if (cmd == "end_for")
            {
                if (_forStack.Count > 0)
                    _currentDepth = _forStack.Pop();
            }
        }
    }

    // ── РАССТАНОВКА СТРОК ──────────────────────────────

    void RefreshLayout()
    {
        int visibleIndex = 0;
        for (int i = 0; i < _lines.Count; i++)
        {
            var rt = _lines[i].obj.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(
                _firstLinePosition.x + _lines[i].depth * _indentOffset,
                _firstLinePosition.y - visibleIndex * _lineSpacing
            );
            visibleIndex++;
        }
    }
}