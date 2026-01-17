using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private GameObject gridCellPrefab;
    [SerializeField] private Color cellColor1 = Color.white;
    [SerializeField] private Color cellColor2 = Color.gray;

    private GameObject[,] gridCells;

    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        gridCells = new GameObject[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 position = new Vector3(x * cellSize, y * cellSize, 0);
                GameObject cell = Instantiate(gridCellPrefab, position, Quaternion.identity, transform);

                // Шахматная раскраска
                SpriteRenderer renderer = cell.GetComponent<SpriteRenderer>();
                renderer.color = (x + y) % 2 == 0 ? cellColor1 : cellColor2;

                // BoxCollider для определения клетки
                cell.AddComponent<BoxCollider2D>();
                cell.name = $"Cell_{x}_{y}";

                gridCells[x, y] = cell;
            }
        }
    }

    public Vector2 GetCellCenter(Vector2Int gridPosition)
    {
        // Проверка выхода за границы
        if (gridPosition.x < 0 || gridPosition.x >= gridWidth || 
            gridPosition.y < 0 || gridPosition.y >= gridHeight)
        {
            Debug.LogError($"Cell position {gridPosition} is out of bounds!");
            return Vector2.zero;
        }
        
        // Вычисляем центр клетки
        float centerX = gridPosition.x * cellSize;
        float centerY = gridPosition.y * cellSize;
        
        return new Vector2(centerX, centerY);
    }
}