using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public interface ICommand
{
    void Execute();
    void Undo();
}

public class ChangeGameStatusCommand : ICommand
{
    private GameStatus _dataGameStatus;
    private GameStatus _lastGameStatus;
    private Game _game;

    public ChangeGameStatusCommand(GameStatus dataGameStatus, Game game)
    {
        _game = game;
        _dataGameStatus = dataGameStatus;
        _lastGameStatus = _game.Status;
    }

    public void Execute()
    {
        _game.Status = _dataGameStatus;
    }

    public void Undo()
    {
        _game.Status = _lastGameStatus;
    }
}

public class SetKingCommand : ICommand
{
    private Figure _dataFigure;
    private bool _enabled;

    public SetKingCommand(Figure figure, bool enabled)
    {
        _dataFigure = figure;
        _enabled = enabled;
    }

    public void Execute()
    {
        _dataFigure.isKing = _enabled;
    }

    public void Undo()
    {
        _dataFigure.isKing = !_enabled;
    }
}

public class SwitchPlayerCommand : ICommand
{
    private Game _game;

    public SwitchPlayerCommand(Game game)
    {
        _game = game;
    }

    public void Execute()
    {
        _game.SwitchCurrentPlayer();
    }

    public void Undo()
    {
        Execute();
    }
}

public class SelectSourceCellCommand : ICommand
{
    private Board _board;
    private BoardSnapshot _snapshot;
    private Cell _dataCell;

    public SelectSourceCellCommand(Board board, Cell cell)
    {
        _board = board;
        _dataCell = cell;
    }

    public void Execute()
    {
        _snapshot = _board.MakeSnapshot();

        if (!_board.CanCellBeSource(_dataCell)) return;

        _board.SelectedCell = _dataCell;
        _board.GoalsFinder.Find(_board.SelectedCell, _board.SelectedCell.figure.isKing);
        if (!_board.GoalsFinder.hasGoals) { _board.ResetSourceCell(); return; }
        _board.HighlightGoals(true);
        _dataCell.transform.DOPunchScale(new Vector3(0, 0, 0.01f), 0.4f);
    }

    public void Undo()
    {
        _board.ApplySnapshot(_snapshot);
        _board.HighlightGoals(false);
    }
}

public class SelectTargetCellCommand : ICommand
{
    public void Execute()
    {
        throw new System.NotImplementedException();
    }

    public void Undo()
    {
        throw new System.NotImplementedException();
    }
}

public class MoveCommand : ICommand
{
    private Board _board;
    private BoardSnapshot _boardSnapshot;
    private Game _game;
    private Step _step;
    private SetKingCommand setKingCommand;

    public MoveCommand(Step step, Board board, Game game)
    {
        _board = board;
        _step = step;
        _game = game;
    }

    public void Execute()
    {
        if ((_game.CurrentPlayer == Player.White && _step.endCell.position.y == _board.GridSize - 1 || _game.CurrentPlayer == Player.Black && _step.endCell.position.y == 0))
        {
            setKingCommand = new SetKingCommand(_step.startCell.figure, true);
            setKingCommand.Execute();
        }

        _step.startCell.figure.transform.DOMove(_step.endCell.transform.position, 0.4f);
        _step.endCell.SetFigure(_step.startCell.figure);
        _step.startCell.SetFigure(null);
        _board.checkerMoveEvent(_step.endCell);
        _boardSnapshot = _board.MakeSnapshot();
        _board.lastDiagonalMove = _board.GetDiagonal(_step.startCell, _step.endCell);
        Debug.Log($"[ {_step.startCell.position.x} , {_step.startCell.position.y} ] -> [ {_step.endCell.position.x} , {_step.endCell.position.y} ] ");
    }

    public void Undo()
    {
        if (setKingCommand != null)
            setKingCommand.Undo();
        _step.endCell.figure.transform.DOMove(_step.startCell.transform.position, 0.4f);
        _step.startCell.SetFigure(_step.endCell.figure);
        _step.endCell.SetFigure(null);
        _board.checkerMoveEvent(_step.startCell);
        _board.HighlightGoals(false);
        _board.ApplySnapshot(_boardSnapshot);
        //_board.SelectSourceCell(_boardSnapshot.GetData().Selected);
        Debug.Log($"[ {_step.endCell.position.x} , {_step.endCell.position.y} ] -> [ {_step.startCell.position.x} , {_step.startCell.position.y} ] ");
    }
}


public class CombatCommand : ICommand
{
    private Combat _dataCombat;
    private Vector3 _enemyPosition;
    private Quaternion _enemyRotation;
    private FigureColor _enemyFigureColor;
    private Material _enemyMaterial;

    private Board _board;
    private Game _game;

    private MoveCommand _moveCommand;

    public CombatCommand(Combat dataCombat, Board board, Game game)
    {
        _dataCombat = dataCombat;
        _enemyPosition = dataCombat.enemyCell.figure.transform.position;
        _enemyRotation = dataCombat.enemyCell.figure.transform.rotation;
        _enemyFigureColor = dataCombat.enemyCell.figure.color;
        _enemyMaterial = dataCombat.enemyCell.figure.material;
        _moveCommand = new MoveCommand(new Step(dataCombat.startCell, dataCombat.endCell, dataCombat.isKing), board, game);
        _board = board;
        _game = game;
    }

    public void Execute()
    {
        _moveCommand.Execute();
        _board.Remove(_dataCombat.enemyCell);
    }

    public void Undo()
    {
        Figure enemy = MonoBehaviour.Instantiate<Figure>(_game.gridConfig.figurePrefab, _enemyPosition, _enemyRotation);
        enemy.SetColor(_enemyFigureColor, _enemyMaterial);
        _dataCombat.enemyCell.SetFigure(enemy);
        _moveCommand.Undo();
    }
}

public class History
{
    private ICommand[] _commands;
    private Game _game;
    private Board _board;
    private int _currentIndex;
    private int _commandsCount;

    public History(Game game, Board board)
    {
        _commands = new ICommand[1024];
        _game = game;
        _board = board;
    }

    public void AddCommand(ICommand command, bool execute = false)
    {
        if (execute) command.Execute();
        _commands[_currentIndex] = command;
        _currentIndex++;
        _commandsCount++;
        Debug.Log($"_currentIndex = {_currentIndex}");
    }

    public void StepNext()
    {
        if (_currentIndex == _commandsCount) return;
        Debug.Log("StepNext()");
        _commands[_currentIndex].Execute();
        _currentIndex++;
    }

    public void StepPrevious()
    {
        if (_currentIndex == 0) return;
        Debug.Log("StepPrevious()");
        _currentIndex--;
        _commands[_currentIndex].Undo();
    }
}


