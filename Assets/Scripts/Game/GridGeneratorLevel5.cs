using UnityEngine;

public class GridGeneratorLevel5 : MonoBehaviour
{
    [Header("Настройки поля")]
    [SerializeField] private GameObject _cellPrefab; // Префаб одной клетки
    [SerializeField] private int _gridWidth = 7;     // Ширина поля
    [SerializeField] private int _gridHeight = 7;    // Высота поля
    [SerializeField] private float _cellSize = 1.0f; // Размер клетки в юнитах
    
    [Header("Специальные клетки")]
    [SerializeField] private Vector2Int _startPosition = new Vector2Int(1, 2);
    [SerializeField] private Vector2Int _finishPosition = new Vector2Int(4, 0);
    [SerializeField] private Vector2Int _startPosition2 = new Vector2Int(5, 5);
    [SerializeField] private Vector2Int _finishPosition2 = new Vector2Int(0, 6);
    
    [Header("Спрайты специальных клеток")]
    [SerializeField] private Sprite _startCellSprite;
    [SerializeField] private Sprite _finishCellSprite;
    [SerializeField] private Sprite _finishCellSprite2; // Добавлено: спрайт второго финиша
    
    [Header("Префабы предметов")]
    [SerializeField] private GameObject _obstaclePrefab;
    [SerializeField] private GameObject _robotPrefab;
    [SerializeField] private GameObject _robotPrefab2;
    [SerializeField] private GameObject _coinPrefab;
    
    [Header("Позиции предметов")]
    [SerializeField] private Vector2Int[] _obstaclePositions = new Vector2Int[]
    {
        new Vector2Int(0, 0),
        new Vector2Int(4, 4)
    };
    
    [Header("Монеты")]
    [SerializeField] private Vector2Int[] _coinPositions = new Vector2Int[]
    {
        new Vector2Int(1, 1),
        new Vector2Int(3, 3),
        new Vector2Int(5, 2)
    };

    void Start()
    {
        GenerateGrid();
        PlaceItems();
        PlaceCoins();
        PlaceRobot();
        PlaceRobot2();
    }

    void GenerateGrid()
    {
        // Проверка на наличие префаба
        if (_cellPrefab == null)
        {
            Debug.LogError("Cell Prefab не назначен в GridGenerator!");
            return;
        }

        // Циклы для создания сетки
        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                // Рассчитываем позицию для новой клетки
                Vector3 cellPosition = new Vector3(x * _cellSize, y * _cellSize, 0);

                // Создаем экземпляр префаба клетки
                GameObject newCell = Instantiate(_cellPrefab, cellPosition, Quaternion.identity);

                // Переименовываем для удобства в Hierarchy
                newCell.name = $"Cell ({x}, {y})";

                // Делаем созданную клетку дочерним объектом этого GameObject (GameBoard)
                newCell.transform.SetParent(transform, false);
                
                // Настраиваем тип клетки (старт/финиш/обычная)
                SetupCellType(newCell, x, y);
            }
        }
    }
    
    void SetupCellType(GameObject cell, int x, int y)
    {
        SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        
        // Проверяем, является ли клетка стартовой или финишной
        if (x == _startPosition.x && y == _startPosition.y && _startCellSprite != null)
        {
            sr.sprite = _startCellSprite;
            cell.name += " (Start1)";
        }
        else if (x == _finishPosition.x && y == _finishPosition.y && _finishCellSprite != null)
        {
            sr.sprite = _finishCellSprite;
            cell.name += " (Finish1)";
        }
        else if (x == _startPosition2.x && y == _startPosition2.y && _startCellSprite != null)
        {
            sr.sprite = _startCellSprite;
            cell.name += " (Start2)";
        }
        else if (x == _finishPosition2.x && y == _finishPosition2.y && _finishCellSprite2 != null) // Используем отдельный спрайт
        {
            sr.sprite = _finishCellSprite2;
            cell.name += " (Finish2)";
        }
        // Если это препятствие - меняем спрайт клетки на спрайт препятствия
        else if (IsObstaclePosition(new Vector2Int(x, y)))
        {
            // Для препятствия можно оставить обычный спрайт или поменять
            // Если хотите менять спрайт клетки для препятствия:
            // sr.sprite = _obstacleSprite; // если добавите такое поле
        }
    }
    
    void PlaceItems()
    {
        // Размещаем препятствия
        foreach (var pos in _obstaclePositions)
        {
            if (_obstaclePrefab != null)
            {
                PlaceItem(_obstaclePrefab, pos, "Obstacle", -0.2f);
            }
        }
    }
    
    void PlaceCoins()
    {
        // Размещаем монеты
        foreach (var pos in _coinPositions)
        {
            if (_coinPrefab != null)
            {
                PlaceItem(_coinPrefab, pos, "Coin", -0.3f);
            }
        }
    }
    
    void PlaceRobot()
    {
        if (_robotPrefab != null)
        {
            Vector3 robotPosition = new Vector3(
                _startPosition.x * _cellSize,
                _startPosition.y * _cellSize - 1.0f,
                -1f
            );
            
            GameObject robot = Instantiate(_robotPrefab, robotPosition, Quaternion.identity);
            robot.name = "Robot1";
            robot.transform.SetParent(transform);
            
            // Для отладки - выводим позиции
            Debug.Log($"Первый робот: стартовая клетка ({_startPosition.x}, {_startPosition.y})");
            Debug.Log($"Робот1 создан в: {robotPosition}");
        }
    }
    
    void PlaceRobot2()
    {
        if (_robotPrefab2 != null)
        {
            Vector3 robotPosition = new Vector3(
                _startPosition2.x * _cellSize,
                _startPosition2.y * _cellSize - 1.0f,
                -1.1f
            );
            
            GameObject robot = Instantiate(_robotPrefab2, robotPosition, Quaternion.identity);
            robot.name = "Robot2";
            robot.transform.SetParent(transform);
            
            // Для отладки - выводим позиции
            Debug.Log($"Второй робот: стартовая клетка ({_startPosition2.x}, {_startPosition2.y})");
            Debug.Log($"Робот2 создан в: {robotPosition}");
        }
    }
    
    void PlaceItem(GameObject prefab, Vector2Int gridPosition, string itemName, float zPosition)
    {
        Vector3 worldPosition = new Vector3(
            gridPosition.x * _cellSize,
            gridPosition.y * _cellSize - 1.0f,
            zPosition  // Предметы поверх клеток
        );
        
        GameObject item = Instantiate(prefab, worldPosition, Quaternion.identity);
        item.name = itemName;
        // Можно сделать предмет дочерним объектом GameBoard
        item.transform.SetParent(transform);
    }
    
    bool IsObstaclePosition(Vector2Int position)
    {
        foreach (var obstaclePos in _obstaclePositions)
        {
            if (obstaclePos.x == position.x && obstaclePos.y == position.y)
                return true;
        }
        return false;
    }

    // Свойство для доступа к размеру клетки из других скриптов
    public float CellSize => _cellSize;

    // Свойства для доступа к параметрам сетки из других скриптов
    public int GridWidth => _gridWidth;
    public int GridHeight => _gridHeight;
    
    // Публичные геттеры для специальных позиций
    public Vector2Int StartPosition => _startPosition;
    public Vector2Int FinishPosition => _finishPosition;
    public Vector2Int StartPosition2 => _startPosition2;
    public Vector2Int FinishPosition2 => _finishPosition2;
}