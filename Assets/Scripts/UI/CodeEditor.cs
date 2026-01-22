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
    [SerializeField] private float _lineSpacing = 10000f; // Увеличил для scale 100
    [SerializeField] private Vector2 _firstLinePosition = new Vector2(150f, -330f);
    [SerializeField] private Vector2 _lineSize = new Vector2(300f, 80f); // Увеличил размер
    
    [Header("Настройки масштаба")]
    [SerializeField] private float _lineScale = 100f; // Масштаб строк (можно менять в инспекторе)
    [SerializeField] private float _zOffset = -10f; // Смещение по Z чтобы строки были поверх
    
    [Header("Кнопки")]
    [SerializeField] private Button _moveForwardButton;
    [SerializeField] private Button _turnLeftButton;
    [SerializeField] private Button _turnRightButton;
    
    private List<GameObject> _codeLines = new List<GameObject>();

    void Start()
    {
        InitializeButtons();
        Debug.Log($"CodeEditor запущен. Масштаб строк: {_lineScale}");
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
        Vector3 linePosition = new Vector3(
            _firstLinePosition.x,
            _firstLinePosition.y - (_codeLines.Count * _lineSpacing),
            _zOffset // Важно: отрицательный Z чтобы быть поверх
        );

        // Создаем строку
        GameObject newLine = Instantiate(linePrefab, _codeLinesContainer);
        newLine.name = $"CodeLine_{lineName}_{_codeLines.Count + 1}";
        
        // Настраиваем RectTransform
        RectTransform rectTransform = newLine.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Настраиваем anchors и pivot
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f); // Центр для scale
            
            // Устанавливаем позицию
            rectTransform.anchoredPosition = new Vector2(linePosition.x, linePosition.y);
            
            // Устанавливаем локальную позицию Z
            Vector3 localPos = rectTransform.localPosition;
            rectTransform.localPosition = new Vector3(localPos.x, localPos.y, linePosition.z);
            
            // Устанавливаем размер
            rectTransform.sizeDelta = _lineSize;
            
            // Устанавливаем МАСШТАБ
            float actualScale = _lineScale / 100f; // 100 в инспекторе = scale 1
            rectTransform.localScale = new Vector3(actualScale, actualScale, 1f);
            
            // Сбрасываем rotation
            rectTransform.localRotation = Quaternion.identity;
            
            // Делаем строку последним дочерним элементом (чтобы была поверх)
            newLine.transform.SetAsLastSibling();
            
            Debug.Log($"Создана строка: {lineName}");
            Debug.Log($"Позиция: {rectTransform.anchoredPosition}, Z={rectTransform.localPosition.z}");
            Debug.Log($"Масштаб: {rectTransform.localScale}");
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
    
    // Метод для отладки видимости
    [ContextMenu("Проверить видимость строк")]
    void CheckLinesVisibility()
    {
        Debug.Log("=== ПРОВЕРКА ВИДИМОСТИ СТРОК ===");
        
        // Проверяем Canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"Canvas: mode={canvas.renderMode}, sortingLayer={canvas.sortingLayerName}, order={canvas.sortingOrder}");
        }
        
        // Проверяем контейнер
        if (_codeLinesContainer != null)
        {
            Debug.Log($"Контейнер Z: {_codeLinesContainer.localPosition.z}");
            
            // Проверяем компоненты рендеринга
            CanvasRenderer containerRenderer = _codeLinesContainer.GetComponent<CanvasRenderer>();
            if (containerRenderer != null)
            {
                Debug.Log($"Контейнер рендерер: cull={containerRenderer.cull}, hasMoved={containerRenderer.hasMoved}");
            }
        }
        
        // Проверяем строки
        foreach (GameObject line in _codeLines)
        {
            if (line != null)
            {
                RectTransform rt = line.GetComponent<RectTransform>();
                Image img = line.GetComponent<Image>();
                
                Debug.Log($"Строка {line.name}:");
                Debug.Log($"  Позиция: {rt.anchoredPosition}, Z={rt.localPosition.z}");
                Debug.Log($"  Масштаб: {rt.localScale}");
                Debug.Log($"  Размер: {rt.sizeDelta}");
                Debug.Log($"  Image enabled: {img.enabled}");
                Debug.Log($"  Активна: {line.activeSelf}, Родитель активен: {line.transform.parent.gameObject.activeSelf}");
            }
        }
    }
    
    // Метод для принудительного исправления Z-координаты
    [ContextMenu("Исправить Z-координаты")]
    void FixZPositions()
    {
        foreach (GameObject line in _codeLines)
        {
            if (line != null)
            {
                RectTransform rt = line.GetComponent<RectTransform>();
                if (rt != null)
                {
                    Vector3 pos = rt.localPosition;
                    rt.localPosition = new Vector3(pos.x, pos.y, _zOffset);
                    line.transform.SetAsLastSibling(); // Перемещаем на передний план
                }
            }
        }
        Debug.Log($"Z-координаты {_codeLines.Count} строк исправлены");
    }
    
    // Метод для принудительного исправления масштаба
    [ContextMenu("Исправить масштаб всех строк")]
    void FixAllLineScales()
    {
        float actualScale = _lineScale / 100f;
        
        foreach (GameObject line in _codeLines)
        {
            if (line != null)
            {
                RectTransform rt = line.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.localScale = new Vector3(actualScale, actualScale, 1f);
                }
            }
        }
        Debug.Log($"Масштаб {_codeLines.Count} строк исправлен на {actualScale}");
    }
}