using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CodeEditorLevel4 : MonoBehaviour
{
    [Header("Спрайты строк кода")]
    [SerializeField] private Sprite _moveForwardSprite;
    [SerializeField] private Sprite _turnLeftSprite;
    [SerializeField] private Sprite _turnRightSprite;
    [SerializeField] private Sprite _forSprite;
    [SerializeField] private Sprite _ifSprite;
    [SerializeField] private Sprite _collectSprite;
    
    [Header("Настройки отображения")]
    [SerializeField] private RectTransform _codeLinesContainer;
    [SerializeField] private float _lineSpacing = 40f;
    [SerializeField] private Vector2 _firstLinePosition = new Vector2(150f, -330f);
    [SerializeField] private Vector2 _lineSize = new Vector2(150f, 30f);
    
    [Header("Настройки масштаба")]
    [SerializeField] private float _lineScale = 1f;
    
    [Header("Кнопки")]
    [SerializeField] private Button _moveForwardButton;
    [SerializeField] private Button _turnLeftButton;
    [SerializeField] private Button _turnRightButton;
    [SerializeField] private Button _forButton;
    [SerializeField] private Button _ifButton;
    [SerializeField] private Button _collectButton;
    [SerializeField] private Button _deleteLastLineButton;
    
    private List<GameObject> _codeLines = new List<GameObject>();

    void Start()
    {
        InitializeButtons();
    }
    
    void InitializeButtons()
    {
        if (_moveForwardButton != null)
            _moveForwardButton.onClick.AddListener(() => AddCodeLine(_moveForwardSprite, "move_forward"));
        
        if (_turnLeftButton != null)
            _turnLeftButton.onClick.AddListener(() => AddCodeLine(_turnLeftSprite, "turn_left"));
        
        if (_turnRightButton != null)
            _turnRightButton.onClick.AddListener(() => AddCodeLine(_turnRightSprite, "turn_right"));
        
        if (_forButton != null)
            _forButton.onClick.AddListener(() => AddCodeLine(_forSprite, "for"));
        
        if (_ifButton != null)
            _ifButton.onClick.AddListener(() => AddCodeLine(_ifSprite, "if"));
        
        if (_collectButton != null)
            _collectButton.onClick.AddListener(() => AddCodeLine(_collectSprite, "collect"));
        
        if (_deleteLastLineButton != null)
            _deleteLastLineButton.onClick.AddListener(DeleteLastLine);
    }
    
    void AddCodeLine(Sprite lineSprite, string lineName)
    {
        if (lineSprite == null)
        {
            Debug.LogWarning($"Спрайт для {lineName} не назначен!");
            return;
        }
        
        if (_codeLinesContainer == null)
        {
            Debug.LogError("Контейнер для строк кода не назначен!");
            return;
        }
        
        // Позиция для новой строки
        float yPos = _firstLinePosition.y - (_codeLines.Count * _lineSpacing);
        Vector2 linePosition = new Vector2(_firstLinePosition.x, yPos);

        // Создаем GameObject для строки
        GameObject newLine = new GameObject($"CodeLine_{lineName}_{_codeLines.Count + 1}");
        newLine.transform.SetParent(_codeLinesContainer, false);
        
        // Добавляем RectTransform
        RectTransform rectTransform = newLine.AddComponent<RectTransform>();
        
        // ВАЖНО: anchors в центре для простого позиционирования
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f); // Pivot в верхнем левом углу
        
        // Устанавливаем позицию (anchoredPosition работает с anchors)
        rectTransform.anchoredPosition = linePosition;
        
        // Устанавливаем размер
        rectTransform.sizeDelta = _lineSize;
        
        // Устанавливаем масштаб
        rectTransform.localScale = new Vector3(_lineScale, _lineScale, 1f);
        
        // Добавляем Image
        Image image = newLine.AddComponent<Image>();
        image.sprite = lineSprite;
        image.preserveAspect = true;
        
        _codeLines.Add(newLine);
    }
    
    void DeleteLastLine()
    {
        if (_codeLines.Count > 0)
        {
            GameObject lastLine = _codeLines[_codeLines.Count - 1];
            if (lastLine != null)
            {
                Destroy(lastLine);
            }
            _codeLines.RemoveAt(_codeLines.Count - 1);
        }
    }
    
    [ContextMenu("Очистить редактор")]
    public void ClearEditor()
    {
        foreach (GameObject line in _codeLines)
        {
            if (line != null)
                Destroy(line);
        }
        _codeLines.Clear();
    }
}