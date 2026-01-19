using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    [SerializeField] private GridGenerator _gridGenerator;
    [SerializeField] private float _screenPart = 0.25f; // На какую долю ширины экрана сместить центр (0.25 = 1/4 слева)

    void Start()
    {
        if (_gridGenerator == null)
        {
            Debug.LogError("GridGenerator не назначен в CameraSetup!");
            return;
        }

        AdjustCamera();
    }

    void AdjustCamera()
    {
        // 1. Находим центр нашего поля в мировых координатах
        float gridCenterX = (_gridGenerator.GridWidth * _gridGenerator.CellSize) / 2f;
        float gridCenterY = (_gridGenerator.GridHeight * _gridGenerator.CellSize) / 2f;
        Vector3 gridCenterWorld = new Vector3(gridCenterX, gridCenterY, -10); // Z = -10 для камеры

        // 2. Переводим эту мировую точку в точку на экране (Viewport)
        Camera cam = Camera.main;
        Vector3 gridCenterViewport = cam.WorldToViewportPoint(gridCenterWorld);
        // Viewport: (0,0) - нижний левый угол экрана, (1,1) - верхний правый.

        // 3. Насколько нам нужно сдвинуть камеру в Viewport координатах?
        // Сейчас центр поля находится в gridCenterViewport.x доле от ширины экрана.
        // Мы хотим, чтобы он был на _screenPart (например, 0.25).
        float viewportOffsetX = _screenPart - gridCenterViewport.x;

        // 4. Переводим смещение из Viewport обратно в мировые координаты
        Vector3 worldOffset = cam.ViewportToWorldPoint(new Vector3(viewportOffsetX, 0, 0)) - cam.ViewportToWorldPoint(Vector3.zero);

        // 5. Сдвигаем камеру на рассчитанное смещение
        cam.transform.Translate(worldOffset, Space.World);
    }
}