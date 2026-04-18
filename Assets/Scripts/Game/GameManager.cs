using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _robotPrefab; // Префаб робота
    [SerializeField] private Vector2Int _robotStartCell = new Vector2Int(0, 2); // Стартовая клетка (X, Y)

    // Ссылка на генератор сетки, чтобы узнать размер клетки
    [SerializeField] private GridGenerator _gridGenerator;

    [SerializeField] private GameData _gameData; // ScriptableObject с данными

    void Start()
    {
        SaveSystem.Load(_gameData); // загружаем сохранение в SO
        //SpawnRobot();
    }

    void SpawnRobot()
    {
        if (_robotPrefab == null || _gridGenerator == null)
        {
            Debug.LogError("Robot Prefab или GridGenerator не назначены в GameManager!");
            return;
        }

        // Рассчитываем мировую позицию для робота.
        // Умножаем координаты клетки на размер клетки.
        float cellSize = _gridGenerator.CellSize; // Нужно добавить public-поле или свойство в GridGenerator

        Vector3 robotWorldPosition = new Vector3(
            _robotStartCell.x * cellSize,
            _robotStartCell.y * cellSize,
            0 // Z-координату можно сделать -1, чтобы робот был "перед" клетками, если они в одной плоскости
        );

        // Создаем робота
        GameObject robot = Instantiate(_robotPrefab, robotWorldPosition, Quaternion.identity);
        robot.name = "PlayerRobot";
    }

    // Вызывай когда уровень пройден
    public void CompleteLevel(int currentLevelIndex)
    {
        if (currentLevelIndex >= _gameData.lastUnlockedLevel)
        {
            _gameData.lastUnlockedLevel = currentLevelIndex + 1;
        }

        SaveSystem.Save(_gameData);
    }
}