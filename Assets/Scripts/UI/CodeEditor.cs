using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CodeEditor : MonoBehaviour
{
    [Header("Префабы строк кода")]
    [SerializeField] private GameObject _moveForwardLinePrefab;
    [SerializeField] private GameObject _turnLeftLinePrefab;
    [SerializeField] private GameObject _turnRightLinePrefab;
    
    [Header("Настройки отображения")]
    [SerializeField] private RectTransform _codeLinesContainer;
    [SerializeField] private float _lineSpacing = 60f; // Увеличил
    [SerializeField] private Vector2 _firstLinePosition = new Vector2(150f, -330f);
    [SerializeField] private Vector2 _lineSize = new Vector2(1000f, 300f); // Размер строки
    
    [Header("Кнопки")]
    [SerializeField] private Button _moveForwardButton;
    [SerializeField] private Button _turnLeftButton;
    [SerializeField] private Button _turnRightButton;
    
    private List<GameObject> _codeLines = new List<GameObject>();

    void Start()
    {
        InitializeButtons();
        
        // Проверяем настройки при старте
        if (_codeLinesContainer != null)
        {
            Debug.Log($"Контейнер: anchoredPosition={_codeLinesContainer.anchoredPosition}, sizeDelta={_codeLinesContainer.sizeDelta}");
            Debug.Log($"Контейнер anchors: min={_codeLinesContainer.anchorMin}, max={_codeLinesContainer.anchorMax}");
            Debug.Log($"Контейнер pivot: {_codeLinesContainer.pivot}");
        }
    }
    
    void InitializeButtons()
    {
        if (_moveForwardButton != null)
            _moveForwardButton.onClick.AddListener(() => AddCodeLine(_moveForwardLinePrefab, "move_forward"));
        
        if (_turnLeftButton != null)
            _turnLeftButton.onClick.AddListener(() => AddCodeLine(_turnLeftLinePrefab, "turn_left"));
        
        if (_turnRightButton != null)
            _turnRightButton.onClick.AddListener(() => AddCodeLine(_turnRightLinePrefab, "turn_right"));
    }
    
    void AddCodeLine(GameObject linePrefab, string lineName)
    {
        if (linePrefab == null)
        {
            Debug.LogWarning($"Префаб для {lineName} не назначен!");
            return;
        }
        
        if (_codeLinesContainer == null)
        {
            Debug.LogError("Контейнер для строк кода не назначен!");
            return;
        }
        
        // Рассчитываем позицию
        Vector2 linePosition = new Vector2(
            _firstLinePosition.x,
            _firstLinePosition.y - (_codeLines.Count * _lineSpacing)
        );
        
        // Создаем строку
        GameObject newLine = Instantiate(linePrefab, _codeLinesContainer);
        newLine.name = $"CodeLine_{lineName}_{_codeLines.Count + 1}";
        
        // Настраиваем RectTransform
        RectTransform rectTransform = newLine.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // ВАЖНО: Сначала настраиваем anchors и pivot
            rectTransform.anchorMin = new Vector2(0f, 1f); // Top-Left
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f); // Pivot в верхнем левом углу
            
            // Затем устанавливаем позицию
            rectTransform.anchoredPosition = linePosition;
            
            // Устанавливаем размер
            rectTransform.sizeDelta = _lineSize;
            
            // Сбрасываем локальные трансформации
            rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0);
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
            
            Debug.Log($"Создана строка: {lineName}");
            Debug.Log($"Позиция: anchoredPosition={rectTransform.anchoredPosition}, localPosition={rectTransform.localPosition}");
            Debug.Log($"Размер: {rectTransform.sizeDelta}, Scale: {rectTransform.localScale}");
        }
        else
        {
            Debug.LogError($"У префаба {lineName} нет RectTransform!");
        }
        
        _codeLines.Add(newLine);
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
        Debug.Log("Редактор кода очищен");
    }
    
    // Метод для отладки - выводит всю информацию о позициях
    [ContextMenu("Показать информацию о позициях")]
    void ShowPositionInfo()
    {
        if (_codeLinesContainer == null) return;
        
        Debug.Log("=== ИНФОРМАЦИЯ О ПОЗИЦИЯХ ===");
        
        // Информация о контейнере
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 containerScreenPos = mainCamera.WorldToScreenPoint(_codeLinesContainer.position);
            Debug.Log($"Контейнер в мировых координатах: {_codeLinesContainer.position}");
            Debug.Log($"Контейнер на экране: {containerScreenPos}");
        }
        
        Debug.Log($"Контейнер anchoredPosition: {_codeLinesContainer.anchoredPosition}");
        Debug.Log($"Контейнер rect: {_codeLinesContainer.rect}");
        Debug.Log($"Контейнер anchors: min={_codeLinesContainer.anchorMin}, max={_codeLinesContainer.anchorMax}");
        Debug.Log($"Контейнер pivot: {_codeLinesContainer.pivot}");
        Debug.Log($"Контейнер offsetMin: {_codeLinesContainer.offsetMin}, offsetMax: {_codeLinesContainer.offsetMax}");
        
        // Информация о строках
        Debug.Log($"Всего строк: {_codeLines.Count}");
        for (int i = 0; i < _codeLines.Count; i++)
        {
            if (_codeLines[i] != null)
            {
                RectTransform rt = _codeLines[i].GetComponent<RectTransform>();
                Debug.Log($"Строка {i}: {_codeLines[i].name}, anchoredPosition: {rt.anchoredPosition}, sizeDelta: {rt.sizeDelta}");
            }
        }
    }
    
    // Метод для принудительного изменения размера строк
    [ContextMenu("Исправить размеры всех строк")]
    void FixAllLineSizes()
    {
        foreach (GameObject line in _codeLines)
        {
            if (line != null)
            {
                RectTransform rt = line.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.sizeDelta = _lineSize;
                    rt.localScale = Vector3.one;
                }
            }
        }
        Debug.Log($"Размеры {_codeLines.Count} строк исправлены");
    }
}