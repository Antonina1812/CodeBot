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

    private GridGenerator _grid;
    private RobotController _robot;

    private bool _isRunning = false;
    private Coroutine _executionCoroutine;

    void Start()
    {
        _grid = FindFirstObjectByType<GridGenerator>();

        if (_grid != null)
        {
            _robot = _grid.Robot1;
            Debug.Log("Robot найден");
        }
        else
        {
            Debug.LogError("GridGenerator НЕ найден");
        }

        _runButton?.onClick.AddListener(RunCode);
    }

    public void RunCode()
    {
        if (_isRunning) return;

        List<string> commands = _codeEditor.GetCommands();
        ResetLevel();

        if (commands.Count == 0) { Debug.Log("Нет команд"); return; }

        LevelUI.Instance?.ResetCommandCounter();

        _executionCoroutine = StartCoroutine(Execute(commands));
    }

    public void StopCode()
    {
        if (_executionCoroutine != null)
            StopCoroutine(_executionCoroutine);
        _isRunning = false;
    }

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

    private IEnumerator Execute(List<string> commands)
    {
        _isRunning = true;
        yield return StartCoroutine(RunBlock(commands, 0));
        _isRunning = false;
        
        Debug.Log($"Выполнено команд: {commands.Count}");
        CheckWinCondition();
    }

    private IEnumerator RunBlock(List<string> commands, int startIndex)
    {
        int i = startIndex;

        while (i < commands.Count)
        {
            string cmd = commands[i];

            if (cmd.StartsWith("for:"))
            {
                int count = int.Parse(cmd.Substring(4));
                int endIndex = FindEndFor(commands, i);

                if (endIndex == -1) { Debug.LogError("Не найден end_for!"); yield break; }

                for (int k = 0; k < count; k++)
                    yield return StartCoroutine(RunBlock(commands, i + 1));

                i = endIndex + 1;
                continue;
            }

            if (cmd == "end_for") yield break;

            // Считаем только реальные команды (не for/end_for)
            LevelUI.Instance?.AddExecutedCommand();
            
            yield return StartCoroutine(ExecuteSingleCommand(cmd));
            i++;
        }
    }

    private int FindEndFor(List<string> commands, int startIndex)
    {
        int depth = 0;
        for (int i = startIndex + 1; i < commands.Count; i++)
        {
            if (commands[i].StartsWith("for:")) depth++;
            else if (commands[i] == "end_for")
            {
                if (depth == 0) return i;
                depth--;
            }
        }
        return -1;
    }

    private IEnumerator ExecuteSingleCommand(string cmd)
    {
        switch (cmd)
        {
            case "move_forward": yield return StartCoroutine(_robot.MoveForward()); break;
            case "turn_left": yield return StartCoroutine(_robot.TurnLeft()); break;
            case "turn_right": yield return StartCoroutine(_robot.TurnRight()); break;
            case "collect": yield return StartCoroutine(_robot.Collect()); break;
            case "insert": yield return StartCoroutine(_robot.Insert()); break;
            default: Debug.LogWarning($"Неизвестная команда: {cmd}"); break;
        }
    }

    private void CheckWinCondition()
    {
        if (_grid == null || _robot == null) return;

        Vector2Int finish = _grid.FinishPosition;
        Vector2 finishWorld = new Vector2(
            _grid.transform.position.x + finish.x * _grid.CellSize,
            _grid.transform.position.y + finish.y * _grid.CellSize
        );

        bool win = Vector2.Distance(_robot.transform.position, finishWorld) < _grid.CellSize * 0.5f;

        if (win) LevelUI.Instance?.ShowWin();
        else LevelUI.Instance?.ShowLose();
    }
}