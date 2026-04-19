using UnityEngine;
using System.Collections;

public class RobotController : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float _moveSpeed = 5f;

    private float   _tileSize   = 1f;
    private int     _gridWidth  = 7;
    private int     _gridHeight = 7;
    private Vector2 _gridOrigin;

    private Sprite _spriteDown;
    private Sprite _spriteUp;
    private Sprite _spriteRight;
    private Sprite _spriteLeft;

    // 0=вверх, 90=вправо, 180=вниз, 270=влево
    private int _directionAngle = 180;

    private SpriteRenderer _spriteRenderer;

    private bool _hasDisk = false;

    public bool IsBusy { get; private set; }

    public void Init(float tileSize, int gridWidth, int gridHeight, Sprite spriteDown, Sprite spriteUp, Sprite spriteRight, Sprite spriteLeft)
    {
        _tileSize       = tileSize;
        _gridWidth      = gridWidth;
        _gridHeight     = gridHeight;
        _gridOrigin     = new Vector2(transform.parent.position.x, transform.parent.position.y);
        _spriteDown     = spriteDown;
        _spriteUp       = spriteUp;
        _spriteRight    = spriteRight;
        _spriteLeft     = spriteLeft;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateSprite();
    }

    private Vector2 FacingDirection => _directionAngle switch
    {
        0   => Vector2.up,
        90  => Vector2.right,
        180 => Vector2.down,
        270 => Vector2.left,
        _   => Vector2.up
    };

    private void UpdateSprite()
    {
        if (_spriteRenderer == null) return;

        _spriteRenderer.flipX = false;
        _spriteRenderer.flipY = false;

        _spriteRenderer.sprite = _directionAngle switch
        {
            0   => _spriteUp   != null ? _spriteUp   : _spriteDown,
            90  => _spriteRight,
            180 => _spriteDown,
            270 => _spriteLeft,
            _   => _spriteDown
        };

        transform.rotation = Quaternion.identity;
    }

    private bool IsObstacleAt(Vector2 worldPos)
    {
        try
        {
            foreach (var obs in GameObject.FindGameObjectsWithTag("Obstacle"))
                if (Vector2.Distance((Vector2)obs.transform.position, worldPos) < _tileSize * 0.5f)
                    return true;
        }
        catch { }
        return false;
    }

    // ── Команды ────────────────────────────────────────

    public IEnumerator MoveForward()
    {
        IsBusy = true;
        Vector2 start  = transform.position;
        Vector2 target = start + FacingDirection * _tileSize;

        int targetX = Mathf.RoundToInt((target.x - _gridOrigin.x) / _tileSize);
        int targetY = Mathf.RoundToInt((target.y - _gridOrigin.y) / _tileSize);

        bool inBounds    = targetX >= 0 && targetX < _gridWidth && targetY >= 0 && targetY < _gridHeight;
        bool hasObstacle = IsObstacleAt(target);

        if (inBounds && !hasObstacle)
            yield return SmoothMove(start, target);
        else if (!inBounds)
            Debug.LogWarning("Робот упёрся в границу поля!");
        else
            Debug.LogWarning("Путь заблокирован препятствием!");

        IsBusy = false;
    }

    public IEnumerator TurnLeft()
    {
        IsBusy = true;
        _directionAngle = (_directionAngle - 90 + 360) % 360;
        UpdateSprite();
        IsBusy = false;
        yield break;
    }

    public IEnumerator TurnRight()
    {
        IsBusy = true;
        _directionAngle = (_directionAngle + 90) % 360;
        UpdateSprite();
        IsBusy = false;
        yield break;
    }

    public IEnumerator Collect()
    {
        IsBusy = true;
        Vector2 robotPos = transform.position;

        try
        {
            foreach (GameObject coin in GameObject.FindGameObjectsWithTag("Coin"))
            {
                if (Vector2.Distance(robotPos, (Vector2)coin.transform.position) < _tileSize * 0.5f)
                {
                    Destroy(coin);
                    Debug.Log("Монета подобрана!");
                    IsBusy = false;
                    yield break;
                }
            }
        }
        catch { }

        try
        {
            foreach (GameObject disk in GameObject.FindGameObjectsWithTag("Disk"))
            {
                if (Vector2.Distance(robotPos, (Vector2)disk.transform.position) < _tileSize * 0.5f)
                {
                    Destroy(disk);
                    _hasDisk = true;
                    Debug.Log("Диск подобран!");
                    IsBusy = false;
                    yield break;
                }
            }
        }
        catch { }

        Debug.LogWarning("Нечего подбирать на этой клетке.");
        IsBusy = false;
        yield break;
    }

    public IEnumerator Insert()
    {
        IsBusy = true;

        if (!_hasDisk)
        {
            Debug.LogWarning("У робота нет диска!");
            IsBusy = false;
            yield break;
        }

        if (_directionAngle != 0)
        {
            Debug.LogWarning("Робот должен смотреть вверх чтобы вставить диск!");
            IsBusy = false;
            yield break;
        }

        Vector2 aboveRobotPos = (Vector2)transform.position + Vector2.up * _tileSize;

        try
        {
            foreach (GameObject computer in GameObject.FindGameObjectsWithTag("Computer"))
            {
                if (Vector2.Distance((Vector2)computer.transform.position, aboveRobotPos) < _tileSize * 0.5f)
                {
                    _hasDisk = false;
                    Debug.Log("Диск вставлен в компьютер!");
                    IsBusy = false;
                    yield break;
                }
            }
        }
        catch { }

        Debug.LogWarning("Компьютер не найден над роботом!");
        IsBusy = false;
        yield break;
    }

    // ── Плавное движение ───────────────────────────────

    private IEnumerator SmoothMove(Vector2 from, Vector2 to)
    {
        float elapsed  = 0f;
        float duration = _tileSize / _moveSpeed;
        float z        = transform.position.z;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector2 p          = Vector2.Lerp(from, to, elapsed / duration);
            transform.position = new Vector3(p.x, p.y, z);
            yield return null;
        }

        transform.position = new Vector3(to.x, to.y, z);
    }
}