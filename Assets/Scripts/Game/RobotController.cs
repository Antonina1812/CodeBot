using UnityEngine;

public class RobotController : MonoBehaviour
{
    [SerializeField] private Vector2Int gridPosition = Vector2Int.zero;
    [SerializeField] private Direction currentDirection = Direction.Up;
    [SerializeField] private float moveSpeed = 5f;
    
    private GridManager gridManager;
    private bool isMoving = false;
    private Vector2 targetPosition;
    
    public enum Direction { Up, Right, Down, Left }

    void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        transform.position = gridManager.GetCellCenter(gridPosition);
        UpdateRotation();
    }

    public void MoveForward()
    {
        if (isMoving) return;
        
        Vector2Int moveVector = GetDirectionVector();
        Vector2Int targetGridPos = gridPosition + moveVector;
        
        // Проверка выхода за границы
        if (IsValidPosition(targetGridPos))
        {
            gridPosition = targetGridPos;
            targetPosition = gridManager.GetCellCenter(targetGridPos);
            isMoving = true;
            
            // Запускаем анимацию движения
            StartCoroutine(MoveToPosition());
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

    private System.Collections.IEnumerator MoveToPosition()
    {
        Vector2 startPos = transform.position;
        float journeyLength = Vector2.Distance(startPos, targetPosition);
        float startTime = Time.time;
        
        while (transform.position != (Vector3)targetPosition)
        {
            float distCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = distCovered / journeyLength;
            transform.position = Vector2.Lerp(startPos, targetPosition, fractionOfJourney);
            yield return null;
        }
        
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
        // Здесь добавить проверку на препятствия
        return pos.x >= 0 && pos.x < 10 && pos.y >= 0 && pos.y < 10;
    }
}