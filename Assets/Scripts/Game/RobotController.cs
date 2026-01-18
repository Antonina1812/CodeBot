using UnityEngine;
using System.Collections;

public class RobotController : MonoBehaviour
{
    [SerializeField] private Vector2Int gridPosition = Vector2Int.zero;
    [SerializeField] private Direction currentDirection = Direction.Up;
    [SerializeField] private float moveSpeed = 5f;
    
    private GridManager gridManager;
    private bool isMoving = false;
    
    public enum Direction { Up, Right, Down, Left }

    void Start()
    {
        // 1. Увеличиваем робота в 2 раза
        transform.localScale = new Vector3(2f, 2f, 1f);
        
        // 2. Делаем робота ярким
        GetComponent<SpriteRenderer>().color = Color.blue;
        
        // 3. Находим GridManager
        gridManager = FindFirstObjectByType<GridManager>();
        
        // 4. Ставим на позицию
        if (gridManager != null)
        {
            transform.position = gridManager.GetCellCenter(gridPosition);
        }
        
        // 5. Поворачиваем
        UpdateRotation();
    }

    public void MoveForward()
    {
        if (isMoving) return;
        
        Vector2Int moveVector = GetDirectionVector();
        Vector2Int targetGridPos = gridPosition + moveVector;
        
        if (IsValidPosition(targetGridPos))
        {
            gridPosition = targetGridPos;
            Vector2 targetPos = gridManager.GetCellCenter(targetGridPos);
            StartCoroutine(MoveToPosition(targetPos));
        }
    }

    public void TurnLeft()
    {
        currentDirection = (Direction)(((int)currentDirection + 3) % 4);
        UpdateRotation();
    }

    public void TurnRight()
    {
        currentDirection = (Direction)(((int)currentDirection + 1) % 4);
        UpdateRotation();
    }

    private IEnumerator MoveToPosition(Vector2 target)
    {
        isMoving = true;
        Vector2 start = transform.position;
        float distance = Vector2.Distance(start, target);
        float time = distance / moveSpeed;
        float elapsed = 0;
        
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector2.Lerp(start, target, elapsed / time);
            yield return null;
        }
        
        transform.position = target;
        isMoving = false;
    }

    private void UpdateRotation()
    {
        float angle = currentDirection switch
        {
            Direction.Up => 0f,
            Direction.Right => -90f,
            Direction.Down => 180f,
            Direction.Left => 90f,
            _ => 0f
        };
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private Vector2Int GetDirectionVector()
    {
        return currentDirection switch
        {
            Direction.Up => Vector2Int.up,
            Direction.Right => Vector2Int.right,
            Direction.Down => Vector2Int.down,
            Direction.Left => Vector2Int.left,
            _ => Vector2Int.up
        };
    }

    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 10 && pos.y >= 0 && pos.y < 10;
    }
}