using UnityEngine;
using System.Collections.Generic;

public class GridGenerator : MonoBehaviour
{
    [Header("Настройки поля")]
    [SerializeField] private GameObject _cellPrefab;
    [SerializeField] private int _gridWidth   = 7;
    [SerializeField] private int _gridHeight  = 7;
    [SerializeField] private float _cellSize  = 1.0f;

    [Header("Специальные клетки")]
    [SerializeField] private Vector2Int _startPosition   = new Vector2Int(1, 2);
    [SerializeField] private Vector2Int _finishPosition  = new Vector2Int(4, 0);
    [SerializeField] private Vector2Int _startPosition2  = new Vector2Int(5, 5);
    [SerializeField] private Vector2Int _finishPosition2 = new Vector2Int(0, 6);

    [Header("Спрайты специальных клеток")]
    [SerializeField] private Sprite _startCellSprite;
    [SerializeField] private Sprite _finishCellSprite;
    [SerializeField] private Sprite _finishCellSprite2;

    [Header("Префабы предметов")]
    [SerializeField] private GameObject _obstaclePrefab;
    [SerializeField] private GameObject _robotPrefab;
    [SerializeField] private GameObject _robotPrefab2;
    [SerializeField] private GameObject _coinPrefab;
    [SerializeField] private GameObject _computerPrefab;
    [SerializeField] private GameObject _diskPrefab;

    [Header("Спрайты робота")]
    [SerializeField] private Sprite _robotSpriteDown;
    [SerializeField] private Sprite _robotSpriteRight;
    [SerializeField] private Sprite _robotSpriteLeft;

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

    [Header("Компьютер и диск")]
    [SerializeField] private Vector2Int _computerPosition = new Vector2Int(3, 3);
    [SerializeField] private Vector2Int _diskPosition     = new Vector2Int(2, 4);

    public RobotController Robot1 { get; private set; }

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

        SpawnDynamicObjects();
    }

    void SpawnDynamicObjects()
    {
        PlaceItems();
        PlaceCoins();
        PlaceComputerAndDisk();
        PlaceRobot();
        PlaceRobot2();
    }

    void GenerateGrid()
    {
        if (_cellPrefab == null)
        {
            Debug.LogError("Cell Prefab не назначен в GridGenerator!");
            return;
        }

        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                Vector3 cellPosition = new Vector3(x * _cellSize, y * _cellSize, 0);
                GameObject newCell   = Instantiate(_cellPrefab, cellPosition, Quaternion.identity);
                newCell.name         = $"Cell ({x}, {y})";
                newCell.transform.SetParent(transform, false);
                SetupCellType(newCell, x, y);
            }
        }
    }

    void SetupCellType(GameObject cell, int x, int y)
    {
        SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        if (x == _startPosition.x && y == _startPosition.y && _startCellSprite != null)
        {
            sr.sprite  = _startCellSprite;
            cell.name += " (Start1)";
        }
        else if (x == _finishPosition.x && y == _finishPosition.y && _finishCellSprite != null)
        {
            sr.sprite  = _finishCellSprite;
            cell.name += " (Finish1)";
            cell.tag   = "Finish";
        }
        else if (x == _startPosition2.x && y == _startPosition2.y && _startCellSprite != null)
        {
            sr.sprite  = _startCellSprite;
            cell.name += " (Start2)";
        }
        else if (x == _finishPosition2.x && y == _finishPosition2.y && _finishCellSprite2 != null)
        {
            sr.sprite  = _finishCellSprite2;
            cell.name += " (Finish2)";
        }
    }

    void PlaceItems()
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
        if (_diskPrefab     != null) PlaceItem(_diskPrefab,     _diskPosition,     "Disk",     -0.25f);
    }

    void PlaceRobot()
    {
        if (_robotPrefab == null) return;

        Vector3 robotPosition = new Vector3(
            _startPosition.x * _cellSize,
            _startPosition.y * _cellSize,
            -0.5f
        );

        GameObject robot = Instantiate(_robotPrefab, robotPosition, Quaternion.identity);
        robot.name = "Robot1";
        robot.transform.SetParent(transform);

        Robot1 = robot.AddComponent<RobotController>();
        Robot1.Init(_cellSize, _gridWidth, _gridHeight, _robotSpriteDown, _robotSpriteRight, _robotSpriteLeft);

        _dynamicObjects.Add(robot);
        Debug.Log($"Робот создан в: {robotPosition}");
    }

    void PlaceRobot2()
    {
        if (_robotPrefab2 == null) return;

        Vector3 robotPosition = new Vector3(
            _startPosition2.x * _cellSize,
            _startPosition2.y * _cellSize,
            -0.5f
        );

        GameObject robot = Instantiate(_robotPrefab2, robotPosition, Quaternion.identity);
        robot.name = "Robot2";
        robot.transform.SetParent(transform);

        _dynamicObjects.Add(robot);
        Debug.Log($"Робот2 создан в: {robotPosition}");
    }

    void PlaceItem(GameObject prefab, Vector2Int gridPosition, string itemName, float zPosition)
    {
        Vector3 worldPosition = new Vector3(
            gridPosition.x * _cellSize,
            gridPosition.y * _cellSize,
            zPosition
        );

        GameObject item = Instantiate(prefab, worldPosition, Quaternion.identity);
        item.name = itemName;
        item.transform.SetParent(transform);

        _dynamicObjects.Add(item);
    }

    public float      CellSize        => _cellSize;
    public int        GridWidth       => _gridWidth;
    public int        GridHeight      => _gridHeight;
    public Vector2Int StartPosition   => _startPosition;
    public Vector2Int FinishPosition  => _finishPosition;
    public Vector2Int StartPosition2  => _startPosition2;
    public Vector2Int FinishPosition2 => _finishPosition2;
}