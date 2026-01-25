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
    [SerializeField] private Sprite _insertSprite;
    [SerializeField] private Sprite _collectSprite;

    [Header("Настройки отображения")]
    [SerializeField] private RectTransform _codeLinesContainer;
    [SerializeField] private float _lineSpacing = 35f;
    [SerializeField] private Vector2 _firstLinePosition = new Vector2(0f, 0f);
    [SerializeField] private Vector2 _lineSize = new Vector2(140f, 35f);

    [Header("Настройки масштаба")]
    [SerializeField] private float _lineScale = 1f;

    [Header("Стрелочки для кнопок")]
    [SerializeField] private Sprite _arrowSprite1; // Первый спрайт стрелочки
    [SerializeField] private Sprite _arrowSprite2; // Второй спрайт стрелочки
    [SerializeField] private float _arrowButtonOffset = 65f; // Смещение кнопки слева

    [Header("Кнопки")]
    [SerializeField] private Button _moveForwardButton;
    [SerializeField] private Button _turnLeftButton;
    [SerializeField] private Button _turnRightButton;
    [SerializeField] private Button _forButton;
    [SerializeField] private Button _insertButton;
    [SerializeField] private Button _collectButton;
    [SerializeField] private Button _deleteLastLineButton;

    private List<GameObject> _codeLines = new List<GameObject>();
    private List<GameObject> _arrowButtons = new List<GameObject>();

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
            _forButton.onClick.AddListener(() => AddCodeLine(_forSprite, "for", true));

        if (_insertButton != null)
            _insertButton.onClick.AddListener(() => AddCodeLine(_insertSprite, "insert"));

        if (_collectButton != null)
            _collectButton.onClick.AddListener(() => AddCodeLine(_collectSprite, "collect"));

        if (_deleteLastLineButton != null)
            _deleteLastLineButton.onClick.AddListener(DeleteLastLine);
    }

    void AddCodeLine(Sprite lineSprite, string lineName, bool needsArrowButton = false)
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

        // Создаем кнопку со стрелочкой для строк for и if
        if (needsArrowButton && _arrowSprite1 != null)
        {
            CreateArrowButton(linePosition, _codeLines.Count - 1);
        }
        else
        {
            _arrowButtons.Add(null); // Добавляем null для сохранения порядка
        }
    }

    void CreateArrowButton(Vector2 linePosition, int lineIndex)
    {
        // Позиция кнопки слева от строки
        Vector2 buttonPosition = new Vector2(linePosition.x - _arrowButtonOffset, linePosition.y);

        // Создаем GameObject для кнопки
        GameObject arrowButton = new GameObject($"ArrowButton_{lineIndex}");
        arrowButton.transform.SetParent(_codeLinesContainer, false);

        // Добавляем RectTransform
        RectTransform rectTransform = arrowButton.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = buttonPosition;
        rectTransform.sizeDelta = _lineSize;
        rectTransform.localScale = new Vector3(_lineScale, _lineScale, 1f);

        // Добавляем Image
        Image image = arrowButton.AddComponent<Image>();
        image.sprite = _arrowSprite1;
        image.preserveAspect = true;

        // Добавляем Button
        Button button = arrowButton.AddComponent<Button>();

        // Сохраняем индекс строки в компоненте кнопки
        ArrowButtonData buttonData = arrowButton.AddComponent<ArrowButtonData>();
        buttonData.lineIndex = lineIndex;

        // Назначаем обработчик клика
        button.onClick.AddListener(() => OnArrowButtonClick(arrowButton));

        _arrowButtons.Add(arrowButton);
    }

    void OnArrowButtonClick(GameObject buttonObject)
    {
        Image buttonImage = buttonObject.GetComponent<Image>();
        if (buttonImage == null) return;

        // Переключаем спрайт
        if (buttonImage.sprite == _arrowSprite1)
        {
            buttonImage.sprite = _arrowSprite2;
        }
        else
        {
            buttonImage.sprite = _arrowSprite1;
        }
    }

    void DeleteLastLine()
    {
        if (_codeLines.Count > 0)
        {
            int lastIndex = _codeLines.Count - 1;

            GameObject lastLine = _codeLines[_codeLines.Count - 1];
            if (lastLine != null)
            {
                Destroy(lastLine);
            }
            _codeLines.RemoveAt(_codeLines.Count - 1);

            // Удаляем связанную кнопку если есть
            if (lastIndex < _arrowButtons.Count && _arrowButtons[lastIndex] != null)
            {
                Destroy(_arrowButtons[lastIndex]);
            }
            _arrowButtons.RemoveAt(lastIndex);
        }
    }
    
    // Вспомогательный компонент для хранения данных кнопки
    private class ArrowButtonData : MonoBehaviour
    {
        public int lineIndex;
    }
}