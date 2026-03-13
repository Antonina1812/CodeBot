using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CodeExecutor : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private CodeEditor _codeEditor;

    [Header("Кнопки")]
    [SerializeField] private Button _runButton;

    private GridGenerator   _grid;
    private RobotController _robot;
    private bool            _isRunning = false;
    private Coroutine       _executionCoroutine;

    void Start()
    {
        _grid = FindFirstObjectByType<GridGenerator>();
        if (_grid != null)
        {
            _robot = _grid.Robot1;
            Debug.Log("CodeExecutor: робот найден через GridGenerator");
        }
        else
        {
            Debug.LogError("CodeExecutor: GridGenerator не найден на сцене!");
        }

        if (_runButton != null) _runButton.onClick.AddListener(RunCode);
    }

    // ── Запуск ─────────────────────────────────────────

    public void RunCode()
    {
        if (_isRunning) return;

        // Всегда сбрасываем уровень перед запуском
        ResetLevel();

        List<string> commands = _codeEditor.GetCommands();
        if (commands.Count == 0)
        {
            Debug.Log("Уровень сброшен.");
            return;
        }

        _executionCoroutine = StartCoroutine(ExecuteCommands(commands));
    }

    public void StopCode()
    {
        if (_executionCoroutine != null)
            StopCoroutine(_executionCoroutine);

        _isRunning = false;
        Debug.Log("Выполнение остановлено.");
    }

    // ── Сброс уровня ───────────────────────────────────

    private void ResetLevel()
    {
        if (_executionCoroutine != null)
        {
            StopCoroutine(_executionCoroutine);
            _executionCoroutine = null;
        }

        _isRunning = false;

        if (_grid != null)
        {
            _grid.ResetLevel();
            _robot = _grid.Robot1;
        }
    }

    // ── Исполнитель ────────────────────────────────────

    private IEnumerator ExecuteCommands(List<string> commands)
    {
        _isRunning = true;
        Debug.Log($"Запуск: {commands.Count} команд");

        foreach (string cmd in commands)
        {
            Debug.Log($"Выполняю: {cmd}");

            switch (cmd)
            {
                case "move_forward":
                    yield return StartCoroutine(_robot.MoveForward());
                    break;

                case "turn_left":
                    yield return StartCoroutine(_robot.TurnLeft());
                    break;

                case "turn_right":
                    yield return StartCoroutine(_robot.TurnRight());
                    break;

                case "collect":
                    yield return StartCoroutine(_robot.Collect());
                    break;

                default:
                    Debug.LogWarning($"Неизвестная команда: {cmd}");
                    break;
            }
        }

        _isRunning = false;
        Debug.Log("Программа завершена.");
    }
}