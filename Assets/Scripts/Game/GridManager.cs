using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 5;
    [SerializeField] private int gridHeight = 5;
    [SerializeField] private float cellSize = 1.5f; // Увеличен размер для робота
    [SerializeField] private GameObject gridCellPrefab;
    
    [Header("Position")]
    [SerializeField] private Vector2 gridPosition = new Vector2(-4f, 0f); // Позиция слева
    
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
                // Позиция клетки
                Vector3 position = new Vector3(
                    gridPosition.x + (x * cellSize),
                    gridPosition.y + (y * cellSize),
                    0
                );
                
                // Создаем клетку
                GameObject cell = Instantiate(gridCellPrefab, position, Quaternion.identity, transform);
                
                // Настраиваем спрайт (чередуем цвета)
                SpriteRenderer renderer = cell.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = Color.white;
                }
                
                // Именуем клетку
                cell.name = $"Cell_{x}_{y}";
                gridCells[x, y] = cell;
            }
        }
        
        Debug.Log($"Grid created at position: {gridPosition}");
    }
    
    public Vector2 GetCellCenter(Vector2Int gridPosition)
    {
        // Проверка границ
        if (gridPosition.x < 0 || gridPosition.x >= gridWidth || 
            gridPosition.y < 0 || gridPosition.y >= gridHeight)
        {
            return Vector2.zero;
        }
        
        // Центр клетки
        float centerX = this.gridPosition.x + (gridPosition.x * cellSize);
        float centerY = this.gridPosition.y + (gridPosition.y * cellSize);
        
        return new Vector2(centerX, centerY);
    }
    
    public bool IsValidPosition(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
               gridPosition.y >= 0 && gridPosition.y < gridHeight;
    }
}