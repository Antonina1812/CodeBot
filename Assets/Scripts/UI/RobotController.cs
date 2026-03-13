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
    private Sprite _spriteRight;
    private Sprite _spriteLeft;

    // 0=вверх, 90=вправо, 180=вниз, 270=влево
    private int _directionAngle = 180;

    private SpriteRenderer _spriteRenderer;

    public bool IsBusy { get; private set; }

    public void Init(float tileSize, int gridWidth, int gridHeight, Sprite spriteDown, Sprite spriteRight, Sprite spriteLeft)
    {
        _tileSize       = tileSize;
        _gridWidth      = gridWidth;
        _gridHeight     = gridHeight;
        _gridOrigin     = new Vector2(transform.parent.position.x, transform.parent.position.y);
        _spriteDown     = spriteDown;
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

        switch (_directionAngle)
        {
            case 0:
                _spriteRenderer.sprite = _spriteDown;
                _spriteRenderer.flipY  = true;
                break;
            case 90:
                _spriteRenderer.sprite = _spriteRight;
                break;
            case 180:
                _spriteRenderer.sprite = _spriteDown;
                break;
            case 270:
                _spriteRenderer.sprite = _spriteLeft;
                break;
        }

        transform.rotation = Quaternion.identity;
    }

    // ── Команды ────────────────────────────────────────

    public IEnumerator MoveForward()
    {
        IsBusy = true;
        Vector2 start  = transform.position;
        Vector2 target = start + FacingDirection * _tileSize;

        int targetX = Mathf.RoundToInt((target.x - _gridOrigin.x) / _tileSize);
        int targetY = Mathf.RoundToInt((target.y - _gridOrigin.y) / _tileSize);

        if (targetX >= 0 && targetX < _gridWidth && targetY >= 0 && targetY < _gridHeight)
        {
            yield return SmoothMove(start, target);
        }
        else
        {
            Debug.LogWarning("Робот упёрся в границу поля!");
        }

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

        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        Vector2 robotPos   = transform.position;
        bool found         = false;

        foreach (GameObject coin in coins)
        {
            Vector2 coinPos = coin.transform.position;
            if (Vector2.Distance(robotPos, coinPos) < _tileSize * 0.5f)
            {
                Destroy(coin);
                Debug.Log("Монета подобрана!");
                found = true;
                break;
            }
        }

        if (!found)
            Debug.LogWarning("Монеты на этой клетке нет.");

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