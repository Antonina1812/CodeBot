using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CodeExecutorLevel5 : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private CodeEditorLevel5 _codeEditor;

    [Header("Кнопки")]
    [SerializeField] private Button _runButton;

    private GridGeneratorLevel5 _grid;
    private RobotController _robot1;
    private RobotController _robot2;

    private bool _isRunning = false;
    private Coroutine _executionCoroutine;

    void Start()
    {
        _grid = FindFirstObjectByType<GridGeneratorLevel5>();

        if (_grid != null)
        {
            _robot1 = _grid.Robot1;
            _robot2 = _grid.Robot2;
            Debug.Log("Оба робота найдены");
        }
        else
        {
            Debug.LogError("GridGeneratorLevel5 НЕ найден!");
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
        if (_executionCoroutine != null) StopCoroutine(_executionCoroutine);
        _isRunning = false;
    }

    private void ResetLevel()
    {
        if (_executionCoroutine != null) { StopCoroutine(_executionCoroutine); _executionCoroutine = null; }
        _isRunning = false;
        if (_grid != null) { _grid.ResetLevel(); _robot1 = _grid.Robot1; _robot2 = _grid.Robot2; }
    }

    private IEnumerator Execute(List<string> commands)
    {
        _isRunning = true;
        yield return StartCoroutine(RunBlock(commands, 0, commands.Count, "both"));
        _isRunning = false;
        CheckWinCondition();
    }

    private IEnumerator RunBlock(List<string> commands, int start, int end, string activeRobot)
    {
        int i = start;
        while (i < end)
        {
            string cmd = commands[i];

            if (cmd.StartsWith("for:"))
            {
                int count = int.Parse(cmd.Substring(4));
                int endIndex = FindMatchingEnd(commands, i, "end_for");
                if (endIndex == -1) { Debug.LogError("Не найден end_for!"); yield break; }
                for (int k = 0; k < count; k++)
                    yield return StartCoroutine(RunBlock(commands, i + 1, endIndex, activeRobot));
                i = endIndex + 1;
                continue;
            }

            if (cmd == "end_for") yield break;

            if (cmd.StartsWith("if_robot:"))
            {
                string color = cmd.Substring(9);
                int endIndex = FindMatchingEnd(commands, i, "end_if");
                if (endIndex == -1) { Debug.LogError("Не найден end_if!"); yield break; }
                yield return StartCoroutine(RunBlock(commands, i + 1, endIndex, color));
                i = endIndex + 1;
                continue;
            }

            if (cmd == "end_if") yield break;

            // Считаем только реальные команды
            LevelUI.Instance?.AddExecutedCommand();

            yield return StartCoroutine(ExecuteCommand(cmd, activeRobot));
            i++;
        }
    }

    private IEnumerator ExecuteCommand(string cmd, string activeRobot)
    {
        if (activeRobot == "both")
        {
            bool r1done = _robot1 == null;
            bool r2done = _robot2 == null;
            if (_robot1 != null) StartCoroutine(RunSingle(_robot1, cmd, () => r1done = true));
            if (_robot2 != null) StartCoroutine(RunSingle(_robot2, cmd, () => r2done = true));
            yield return new WaitUntil(() => r1done && r2done);
        }
        else if (activeRobot == "blue" && _robot1 != null) 
            yield return StartCoroutine(ExecuteSingleCommand(_robot1, cmd));
        else if (activeRobot == "yellow" && _robot2 != null) 
            yield return StartCoroutine(ExecuteSingleCommand(_robot2, cmd));
    }

    private IEnumerator RunSingle(RobotController robot, string cmd, System.Action onDone)
    {
        yield return StartCoroutine(ExecuteSingleCommand(robot, cmd));
        onDone?.Invoke();
    }

    private int FindMatchingEnd(List<string> commands, int startIndex, string endTag)
    {
        string openTag = endTag == "end_for" ? "for:" : "if_robot:";
        int depth = 0;
        for (int i = startIndex + 1; i < commands.Count; i++)
        {
            if (commands[i].StartsWith(openTag)) depth++;
            else if (commands[i] == endTag) { if (depth == 0) return i; depth--; }
        }
        return -1;
    }

    private IEnumerator ExecuteSingleCommand(RobotController robot, string cmd)
    {
        switch (cmd)
        {
            case "move_forward": yield return StartCoroutine(robot.MoveForward()); break;
            case "turn_left": yield return StartCoroutine(robot.TurnLeft()); break;
            case "turn_right": yield return StartCoroutine(robot.TurnRight()); break;
            case "collect": yield return StartCoroutine(robot.Collect()); break;
            case "insert": yield return StartCoroutine(robot.Insert()); break;
            default: Debug.LogWarning($"Неизвестная команда: {cmd}"); break;
        }
    }

    private void CheckWinCondition()
    {
        if (_grid == null) return;

        bool robot1Win = IsRobotAtFinish(_robot1, _grid.FinishPosition1);
        bool robot2Win = IsRobotAtFinish(_robot2, _grid.FinishPosition2);
        bool allDone = (_robot1 == null || robot1Win) && (_robot2 == null || robot2Win);

        if (allDone) LevelUI.Instance?.ShowWin();
        else LevelUI.Instance?.ShowLose();
    }

    private bool IsRobotAtFinish(RobotController robot, Vector2Int finish)
    {
        if (robot == null) return false;
        Vector2 finishWorld = new Vector2(
            _grid.transform.position.x + finish.x * _grid.CellSize,
            _grid.transform.position.y + finish.y * _grid.CellSize
        );
        return Vector2.Distance(robot.transform.position, finishWorld) < _grid.CellSize * 0.5f;
    }
}