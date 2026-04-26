using UnityEngine;
using System.Collections.Generic;

public class GridGeneratorLevel5 : MonoBehaviour
{
    [Header("Настройки поля")]
    [SerializeField] private GameObject _cellPrefab;
    [SerializeField] private int _gridWidth = 7;
    [SerializeField] private int _gridHeight = 7;
    [SerializeField] private float _cellSize = 1.0f;

    [Header("Специальные клетки")]
    [SerializeField] private Vector2Int _startPosition1 = new Vector2Int(1, 2);
    [SerializeField] private Vector2Int _startPosition2 = new Vector2Int(5, 2);
    [SerializeField] private Vector2Int _finishPosition1 = new Vector2Int(4, 0);
    [SerializeField] private Vector2Int _finishPosition2 = new Vector2Int(2, 6);

    [Header("Спрайты клеток")]
    [SerializeField] private Sprite _startCellSprite;
    [SerializeField] private Sprite _finishCellSprite1;
    [SerializeField] private Sprite _finishCellSprite2;

    [Header("Префабы")]
    [SerializeField] private GameObject _cellPrefabObj;
    [SerializeField] private GameObject _obstaclePrefab;
    [SerializeField] private GameObject _robotPrefab1;
    [SerializeField] private GameObject _robotPrefab2;
    [SerializeField] private GameObject _coinPrefab;
    [SerializeField] private GameObject _computerPrefab;
    [SerializeField] private GameObject _diskPrefab;

    [Header("Спрайты синего робота (Robot1)")]
    [SerializeField] private Sprite _robot1SpriteDown;
    [SerializeField] private Sprite _robot1SpriteUp;
    [SerializeField] private Sprite _robot1SpriteRight;
    [SerializeField] private Sprite _robot1SpriteLeft;

    [Header("Спрайты жёлтого робота (Robot2)")]
    [SerializeField] private Sprite _robot2SpriteDown;
    [SerializeField] private Sprite _robot2SpriteUp;
    [SerializeField] private Sprite _robot2SpriteRight;
    [SerializeField] private Sprite _robot2SpriteLeft;

    [Header("Позиции препятствий")]
    [SerializeField]
    private Vector2Int[] _obstaclePositions = new Vector2Int[]
    {
        new Vector2Int(2, 2),
        new Vector2Int(4, 4)
    };

    [Header("Монеты")]
    [SerializeField]
    private Vector2Int[] _coinPositions = new Vector2Int[]
    {
        new Vector2Int(1, 1),
        new Vector2Int(5, 5)
    };

    [Header("Компьютер и диск")]
    [SerializeField] private Vector2Int _computerPosition = new Vector2Int(3, 4);
    [SerializeField] private Vector2Int _diskPosition = new Vector2Int(3, 1);

    public RobotController Robot1 { get; private set; }
    public RobotController Robot2 { get; private set; }

    private List<GameObject> _dynamicObjects = new List<GameObject>();

    void Awake()
    {
        GenerateGrid();
        SpawnDynamicObjects();
    }

    public void ResetLevel()
    {
        foreach (var obj in _dynamicObjects)
            if (obj != null) Destroy(obj);

        _dynamicObjects.Clear();
        Robot1 = null;
        Robot2 = null;

        SpawnDynamicObjects();
    }

    void SpawnDynamicObjects()
    {
        PlaceObstacles();
        PlaceCoins();
        PlaceComputerAndDisk();
        PlaceRobot1();
        PlaceRobot2();
    }

    void GenerateGrid()
    {
        if (_cellPrefab == null)
        {
            Debug.LogError("Cell Prefab не назначен в GridGeneratorLevel5!");
            return;
        }

        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                Vector3 cellPos = new Vector3(x * _cellSize, y * _cellSize, 0);
                GameObject newCell = Instantiate(_cellPrefab, cellPos, Quaternion.identity);
                newCell.name = $"Cell ({x}, {y})";
                newCell.transform.SetParent(transform, false);
                SetupCellType(newCell, x, y);
            }
        }
    }

    void SetupCellType(GameObject cell, int x, int y)
    {
        SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        if (x == _startPosition1.x && y == _startPosition1.y && _startCellSprite != null)
        {
            sr.sprite = _startCellSprite;
            cell.name += " (Start1)";
        }
        else if (x == _startPosition2.x && y == _startPosition2.y && _startCellSprite != null)
        {
            sr.sprite = _startCellSprite;
            cell.name += " (Start2)";
        }
        else if (x == _finishPosition1.x && y == _finishPosition1.y && _finishCellSprite1 != null)
        {
            sr.sprite = _finishCellSprite1;
            cell.name += " (Finish1)";
            cell.tag = "Finish";
        }
        else if (x == _finishPosition2.x && y == _finishPosition2.y && _finishCellSprite2 != null)
        {
            sr.sprite = _finishCellSprite2;
            cell.name += " (Finish2)";
        }
    }

    void PlaceObstacles()
    {
        foreach (var pos in _obstaclePositions)
            if (_obstaclePrefab != null)
                PlaceItem(_obstaclePrefab, pos, "Obstacle", -0.2f);
    }

    void PlaceCoins()
    {
        foreach (var pos in _coinPositions)
            if (_coinPrefab != null)
                PlaceItem(_coinPrefab, pos, "Coin", -0.3f);
    }

    void PlaceComputerAndDisk()
    {
        if (_computerPrefab != null) PlaceItem(_computerPrefab, _computerPosition, "Computer", -0.25f);
        if (_diskPrefab != null) PlaceItem(_diskPrefab, _diskPosition, "Disk", -0.25f);
    }

    void PlaceRobot1()
    {
        if (_robotPrefab1 == null) return;

        Vector3 pos = new Vector3(
            transform.position.x + _startPosition1.x * _cellSize,
            transform.position.y + _startPosition1.y * _cellSize,
            -0.5f
        );

        GameObject robot = Instantiate(_robotPrefab1, pos, Quaternion.identity);
        robot.name = "Robot1_Blue";
        robot.transform.SetParent(transform);

        Robot1 = robot.AddComponent<RobotController>();
        Robot1.Init(_cellSize, _gridWidth, _gridHeight,
                    _robot1SpriteDown, _robot1SpriteUp,
                    _robot1SpriteRight, _robot1SpriteLeft);

        _dynamicObjects.Add(robot);
        Debug.Log($"Синий робот создан в: {pos}");
    }

    void PlaceRobot2()
    {
        if (_robotPrefab2 == null) return;

        Vector3 pos = new Vector3(
            transform.position.x + _startPosition2.x * _cellSize,
            transform.position.y + _startPosition2.y * _cellSize,
            -0.5f
        );

        GameObject robot = Instantiate(_robotPrefab2, pos, Quaternion.identity);
        robot.name = "Robot2_Yellow";
        robot.transform.SetParent(transform);

        Robot2 = robot.AddComponent<RobotController>();
        Robot2.Init(_cellSize, _gridWidth, _gridHeight,
                    _robot2SpriteDown, _robot2SpriteUp,
                    _robot2SpriteRight, _robot2SpriteLeft);

        _dynamicObjects.Add(robot);
        Debug.Log($"Жёлтый робот создан в: {pos}");
    }

    void PlaceItem(GameObject prefab, Vector2Int gridPos, string itemName, float z)
    {
        Vector3 worldPos = new Vector3(
            transform.position.x + gridPos.x * _cellSize,
            transform.position.y + gridPos.y * _cellSize,
            z
        );

        GameObject item = Instantiate(prefab, worldPos, Quaternion.identity);
        item.name = itemName;
        item.transform.SetParent(transform);
        _dynamicObjects.Add(item);
    }

    // ── Геттеры ─────────────────────────────────────────
    public float CellSize => _cellSize;
    public int GridWidth => _gridWidth;
    public int GridHeight => _gridHeight;
    public Vector2Int StartPosition1 => _startPosition1;
    public Vector2Int StartPosition2 => _startPosition2;
    public Vector2Int FinishPosition1 => _finishPosition1;
    public Vector2Int FinishPosition2 => _finishPosition2;
}