using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [Header("Настройки поля")]
    [SerializeField] private GameObject _cellPrefab; // Префаб одной клетки
    [SerializeField] private int _gridWidth = 5;     // Ширина поля
    [SerializeField] private int _gridHeight = 5;    // Высота поля
    [SerializeField] private float _cellSize = 1.0f; // Размер клетки в юнитах
    
    [Header("Специальные клетки")]
    [SerializeField] private Vector2Int _startPosition = new Vector2Int(1, 2);
    [SerializeField] private Vector2Int _finishPosition = new Vector2Int(4, 0);
    
    [Header("Спрайты специальных клеток")]
    [SerializeField] private Sprite _startCellSprite;
    [SerializeField] private Sprite _finishCellSprite;
    
    [Header("Префабы предметов")]
    [SerializeField] private GameObject _computerPrefab;
    [SerializeField] private GameObject _diskPrefab;
    [SerializeField] private GameObject _obstaclePrefab;
    [SerializeField] private GameObject _robotPrefab;
    
    [Header("Позиции предметов")]
    [SerializeField] private Vector2Int _computerPosition = new Vector2Int(3, 3);
    [SerializeField] private Vector2Int _diskPosition = new Vector2Int(2, 4);
    [SerializeField] private Vector2Int[] _obstaclePositions = new Vector2Int[]
    {
        new Vector2Int(0, 0),
        new Vector2Int(4, 4)
    };

    void Start()
    {
        GenerateGrid();
        PlaceItems();
        PlaceRobot();
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
            cell.name += " (Start)";
        }
        else if (x == _finishPosition.x && y == _finishPosition.y && _finishCellSprite != null)
        {
            sr.sprite = _finishCellSprite;
            cell.name += " (Finish)";
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
        // Размещаем компьютер
        if (_computerPrefab != null)
        {
            PlaceItem(_computerPrefab, _computerPosition, "Computer");
        }
        
        // Размещаем диск
        if (_diskPrefab != null)
        {
            PlaceItem(_diskPrefab, _diskPosition, "Disk");
        }
        
        // Размещаем препятствия
        foreach (var pos in _obstaclePositions)
        {
            if (_obstaclePrefab != null)
            {
                PlaceItem(_obstaclePrefab, pos, "Obstacle");
            }
        }
    }
    
    void PlaceRobot()
    {
        if (_robotPrefab != null)
        {
            Vector3 robotPosition = new Vector3(
                _startPosition.x * _cellSize + 1.0f,
                _startPosition.y * _cellSize,
                -1f
            );
            
            GameObject robot = Instantiate(_robotPrefab, robotPosition, Quaternion.identity);
            robot.name = "Robot";
            robot.transform.SetParent(transform);
            
            // Для отладки - выводим позиции
            Debug.Log($"Стартовая клетка: ({_startPosition.x}, {_startPosition.y})");
            Debug.Log($"Робот создан в: {robotPosition}");
        }
    }
    
    void PlaceItem(GameObject prefab, Vector2Int gridPosition, string itemName)
    {
        Vector3 worldPosition = new Vector3(
            gridPosition.x * _cellSize + 1.0f,
            gridPosition.y * _cellSize,
            -0.5f  // Предметы поверх клеток
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
}