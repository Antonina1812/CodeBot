using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CodeEditorLevel5 : MonoBehaviour
{
    [Header("Спрайты команд")]
    [SerializeField] private Sprite _moveForwardSprite;
    [SerializeField] private Sprite _turnLeftSprite;
    [SerializeField] private Sprite _turnRightSprite;
    [SerializeField] private Sprite _collectSprite;
    [SerializeField] private Sprite _forOpenSprite;
    [SerializeField] private Sprite _forCloseSprite;
    [SerializeField] private Sprite _ifRobotSprite;

    [Header("UI")]
    [SerializeField] private RectTransform _codeLinesContainer;
    [SerializeField] private float   _lineSpacing       = 35f;
    [SerializeField] private Vector2 _firstLinePosition = new Vector2(10f, -10f);
    [SerializeField] private Vector2 _lineSize          = new Vector2(150f, 30f);
    [SerializeField] private float   _lineScale         = 1f;
    [SerializeField] private float   _indentOffset      = 20f;

    [Header("FOR text")]
    [SerializeField] private Color _forKeywordColor  = new Color(0.4f, 0.7f, 1f);
    [SerializeField] private Color _forVariableColor = new Color(1f, 0.6f, 0.2f);
    [SerializeField] private Color _forNumberColor   = new Color(0.5f, 1f, 0.5f);
    [SerializeField] private float _forLineFontSize  = 14f;

    [Header("Цвета роботов")]
    [SerializeField] private Color _blueRobotColor   = new Color(0.2f, 0.5f, 1f);
    [SerializeField] private Color _yellowRobotColor = new Color(1f, 0.85f, 0f);

    [Header("Кнопки")]
    [SerializeField] private Button _moveForwardButton;
    [SerializeField] private Button _turnLeftButton;
    [SerializeField] private Button _turnRightButton;
    [SerializeField] private Button _collectButton;
    [SerializeField] private Button _forOpenButton;
    [SerializeField] private Button _ifRobotButton;
    [SerializeField] private Button _deleteLastLineButton;

    [Header("Поле ввода числа цикла")]
    [SerializeField] private TMP_InputField _forCountInput;

    private GameObject _colorPickerPanel;

    private class CodeLineData
    {
        public GameObject obj;
        public int        depth;
        public bool       isBlock;  
        public bool       isClosed;
        public Image      arrowImage;
        public Button     arrowButton;
    }

    private List<CodeLineData> _lines    = new List<CodeLineData>();
    private List<string>       _commands = new List<string>();

    private int        _currentDepth = 0;
    private Stack<int> _blockStack   = new Stack<int>(); 

    void Start()
    {
        _moveForwardButton?.onClick.AddListener(() => AddCodeLine(_moveForwardSprite, "move_forward"));
        _turnLeftButton?.onClick.AddListener(()    => AddCodeLine(_turnLeftSprite,    "turn_left"));
        _turnRightButton?.onClick.AddListener(()   => AddCodeLine(_turnRightSprite,   "turn_right"));
        _collectButton?.onClick.AddListener(()     => AddCodeLine(_collectSprite,     "collect"));
        _forOpenButton?.onClick.AddListener(AddForLine);
        _ifRobotButton?.onClick.AddListener(ShowColorPicker);
        _deleteLastLineButton?.onClick.AddListener(DeleteLastLine);

        CreateColorPickerPanel();
    }

    void CreateColorPickerPanel()
    {
        if (_ifRobotButton == null) return;

        _colorPickerPanel = new GameObject("ColorPickerPanel");
        _colorPickerPanel.transform.SetParent(_ifRobotButton.transform.parent, false);

        var ifRT   = _ifRobotButton.GetComponent<RectTransform>();
        var panelRT = _colorPickerPanel.AddComponent<RectTransform>();
        panelRT.anchorMin        = ifRT.anchorMin;
        panelRT.anchorMax        = ifRT.anchorMax;
        panelRT.pivot            = ifRT.pivot;
        panelRT.anchoredPosition = new Vector2(ifRT.anchoredPosition.x,
                                               ifRT.anchoredPosition.y + ifRT.sizeDelta.y + 5f);
        panelRT.sizeDelta = new Vector2(ifRT.sizeDelta.x, ifRT.sizeDelta.y);

        CreateColorButton(_colorPickerPanel.transform, "BlueBtn",   _blueRobotColor,
                          new Vector2(0f, 0f), new Vector2(0.5f, 1f), "blue");
        CreateColorButton(_colorPickerPanel.transform, "YellowBtn", _yellowRobotColor,
                          new Vector2(0.5f, 0f), new Vector2(1f, 1f), "yellow");

        _colorPickerPanel.SetActive(false);
    }

    void CreateColorButton(Transform parent, string name, Color color,
                           Vector2 anchorMin, Vector2 anchorMax, string robotColor)
    {
        GameObject btn = new GameObject(name);
        btn.transform.SetParent(parent, false);

        var rt = btn.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(2f, 2f);
        rt.offsetMax = new Vector2(-2f, -2f);

        var img    = btn.AddComponent<Image>();
        img.color  = color;

        var button = btn.AddComponent<Button>();
        button.targetGraphic = img;
        button.onClick.AddListener(() =>
        {
            _colorPickerPanel.SetActive(false);
            AddIfRobotLine(robotColor);
        });
    }

    void ShowColorPicker()
    {
        if (_colorPickerPanel == null) return;
        _colorPickerPanel.SetActive(!_colorPickerPanel.activeSelf);
    }


    void AddIfRobotLine(string robotColor)
    {
        int depth = _currentDepth;

        GameObject line = new GameObject($"if_robot_{robotColor}");
        line.transform.SetParent(_codeLinesContainer, false);

        var rt = line.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
        rt.sizeDelta  = _lineSize;
        rt.localScale = new Vector3(_lineScale, _lineScale, 1f);

        var img = line.AddComponent<Image>();
        img.sprite         = _ifRobotSprite;
        img.preserveAspect = true;

        GameObject colorSquare = new GameObject("ColorSquare");
        colorSquare.transform.SetParent(line.transform, false);

        var sqRT = colorSquare.AddComponent<RectTransform>();
        sqRT.anchorMin        = new Vector2(1f, 0.1f);
        sqRT.anchorMax        = new Vector2(1f, 0.9f);
        sqRT.pivot            = new Vector2(1f, 0.5f);
        sqRT.anchoredPosition = new Vector2(-4f, 0f);
        sqRT.sizeDelta        = new Vector2(20f, 0f);

        var sqImg = colorSquare.AddComponent<Image>();
        sqImg.color = robotColor == "blue" ? _blueRobotColor : _yellowRobotColor;

        GameObject arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(line.transform, false);

        var arrowRT = arrowObj.AddComponent<RectTransform>();
        arrowRT.anchorMin = arrowRT.anchorMax = arrowRT.pivot = new Vector2(0f, 0.5f);
        arrowRT.anchoredPosition = new Vector2(2f, -_lineSize.y * 0.5f);
        arrowRT.sizeDelta = new Vector2(16f, 16f);

        var arrowImg = arrowObj.AddComponent<Image>();
        arrowImg.sprite = _forOpenSprite;

        var arrowBtn = arrowObj.AddComponent<Button>();

        CodeLineData data = new CodeLineData
        {
            obj         = line,
            depth       = depth,
            isBlock     = true,
            isClosed    = false,
            arrowImage  = arrowImg,
            arrowButton = arrowBtn
        };

        arrowBtn.onClick.AddListener(() => CloseBlock(data, $"end_if"));

        _lines.Add(data);
        _commands.Add($"if_robot:{robotColor}");

        _blockStack.Push(_currentDepth);
        _currentDepth++;

        RefreshLayout();
    }


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
        rt.sizeDelta  = _lineSize;
        rt.localScale = new Vector3(_lineScale, _lineScale, 1f);

        var text = line.AddComponent<TextMeshProUGUI>();
        text.richText = true;
        text.fontSize = _forLineFontSize;

        string kw  = ColorUtility.ToHtmlStringRGB(_forKeywordColor);
        string vr  = ColorUtility.ToHtmlStringRGB(_forVariableColor);
        string num = ColorUtility.ToHtmlStringRGB(_forNumberColor);
        text.text = $"  <color=#{kw}>for</color> <color=#{vr}>i</color> <color=#{kw}>in range</color>(<color=#{num}>{count}</color>):";

        GameObject arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(line.transform, false);

        var arrowRT = arrowObj.AddComponent<RectTransform>();
        arrowRT.anchorMin = arrowRT.anchorMax = arrowRT.pivot = new Vector2(0f, 0.5f);
        arrowRT.anchoredPosition = new Vector2(2f, -_lineSize.y * 0.5f);
        arrowRT.sizeDelta = new Vector2(16f, 16f);

        var arrowImg = arrowObj.AddComponent<Image>();
        arrowImg.sprite = _forOpenSprite;

        var arrowBtn = arrowObj.AddComponent<Button>();

        CodeLineData data = new CodeLineData
        {
            obj         = line,
            depth       = depth,
            isBlock     = true,
            isClosed    = false,
            arrowImage  = arrowImg,
            arrowButton = arrowBtn
        };

        arrowBtn.onClick.AddListener(() => CloseBlock(data, "end_for"));

        _lines.Add(data);
        _commands.Add($"for:{count}");

        _blockStack.Push(_currentDepth);
        _currentDepth++;

        RefreshLayout();
    }

    void CloseBlock(CodeLineData blockData, string endCmd)
    {
        if (blockData.isClosed) return;

        blockData.isClosed = true;
        blockData.arrowImage.sprite = _forCloseSprite;
        blockData.arrowButton.interactable = false;

        _commands.Add(endCmd);

        if (_blockStack.Count > 0)
            _currentDepth = _blockStack.Pop();
        else
            _currentDepth = Mathf.Max(0, _currentDepth - 1);
    }


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


    void DeleteLastLine()
    {
        if (_lines.Count == 0) return;

        _colorPickerPanel?.SetActive(false);

        int last        = _lines.Count - 1;
        CodeLineData ld = _lines[last];

        if (ld.isBlock)
        {
            if (ld.isClosed)
                for (int i = _commands.Count - 1; i >= 0; i--)
                    if (_commands[i] == "end_for" || _commands[i] == "end_if")
                    { _commands.RemoveAt(i); break; }

            for (int i = _commands.Count - 1; i >= 0; i--)
                if (_commands[i].StartsWith("for:") || _commands[i].StartsWith("if_robot:"))
                { _commands.RemoveAt(i); break; }
        }
        else
        {
            for (int i = _commands.Count - 1; i >= 0; i--)
                if (_commands[i] != "end_for" && _commands[i] != "end_if")
                { _commands.RemoveAt(i); break; }
        }

        Destroy(ld.obj);
        _lines.RemoveAt(last);

        RecalculateDepth();
        RefreshLayout();
    }

    void RecalculateDepth()
    {
        _currentDepth = 0;
        _blockStack.Clear();

        foreach (string cmd in _commands)
        {
            if (cmd.StartsWith("for:") || cmd.StartsWith("if_robot:"))
            {
                _blockStack.Push(_currentDepth);
                _currentDepth++;
            }
            else if (cmd == "end_for" || cmd == "end_if")
            {
                if (_blockStack.Count > 0)
                    _currentDepth = _blockStack.Pop();
            }
        }
    }

    void RefreshLayout()
    {
        for (int i = 0; i < _lines.Count; i++)
        {
            var r = _lines[i].obj.GetComponent<RectTransform>();
            r.anchoredPosition = new Vector2(
                _firstLinePosition.x + _lines[i].depth * _indentOffset,
                _firstLinePosition.y - i * _lineSpacing
            );
        }
    }

    public List<string> GetCommands()
    {
        List<string> result = new List<string>(_commands);
        int depth = 0;
        foreach (string cmd in result)
        {
            if (cmd.StartsWith("for:") || cmd.StartsWith("if_robot:")) depth++;
            else if (cmd == "end_for" || cmd == "end_if") depth--;
        }
        while (depth-- > 0) result.Add("end_if");
        return result;
    }

    public void ClearAll()
    {
        foreach (var line in _lines)
            if (line.obj != null) Destroy(line.obj);
        _lines.Clear();
        _commands.Clear();
        _currentDepth = 0;
        _blockStack.Clear();
        _colorPickerPanel?.SetActive(false);
    }
}