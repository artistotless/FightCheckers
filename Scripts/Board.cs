using System;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using static SwitchPlayerCommand;

public enum MoveResult
{
    Prohibited,                 // запрещено
    Step,            // разрешённый простой ход
    Combat           // разрешённое взятие шашки противника
}

public enum GoalDirection
{
    BottomLeft,
    BottomRight,
    TopRight,
    TopLeft,
}

public class Board
{
    public Action<Cell> checkerMoveEvent;
    public Vector2? lastDiagonalMove = null;
    public Cell SelectedCell;

    public int GridSize { get; private set; }
    public GoalsFinder GoalsFinder { get; private set; }

    private CellsGrid _grid;
    private Game _game;

    public Board(CellsGrid grid, Game game)
    {
        _grid = grid;
        _game = game;
        GoalsFinder = new GoalsFinder(this);
        _grid.Generate();
        GridSize = _grid.size;
        _game.history = new History(game, this);
    }

    public bool HasCombat(Cell cell)
    {
        bool result = false;
        if (cell.isProhibit()) return result;
        if (!_game.IsMyTurn(cell)) { Debug.Log("Не ваша очередь (HasCombat)"); return result; }
        CombatRouter router = GoalsFinder.GetCombatRouter();
        if (router.Routes.Count == 0)
            return HasGoals(cell, true);

        if (GoalsFinder.GetCombatRouter().isEndCell(cell))
            return HasGoals(cell, true);
        return true;
    }

    public void HighlightGoals(bool enabled = true)
    {
        GoalsFinder.HighlightGoals(enabled);
    }

    private bool HasGoals(Cell cell, bool combat = false)
    {
        List<Cell> steps = new List<Cell>();
        List<Cell> battles = new List<Cell>();

        GoalsFinder.FindAndFillResults(cell, cell.figure.isKing, battles, steps);

        bool result = combat
            ? battles.Count > 0
            : steps.Count > 0 || battles.Count > 0;
        return result;
    }

    public MoveResult CheckMove(Cell startCell, Cell targetCell)
    {
        MoveResult result = MoveResult.Prohibited;
        if (!CanCheckerMove(startCell, targetCell)) return result;

        float dX = targetCell.position.x - startCell.position.x;
        float dY = targetCell.position.y - startCell.position.y;

        if (Math.Abs(dX) == 2 && Math.Abs(dY) == 2)
        {
            // поиск "промежуточной" ячейки
            Vector2 enemyPose = new Vector2((startCell.position.x + targetCell.position.x) / 2,
                                        (startCell.position.y + targetCell.position.y) / 2);
            State enemyCellState = (_grid.cells[(int)enemyPose.x, (int)enemyPose.y]).State;

            result = targetCell.State != enemyCellState && startCell.State != enemyCellState
                        ? MoveResult.Combat : result;
        }

        else if (Math.Abs(dX) == 1 && dY == -1 && _game.CurrentPlayer == Player.Black ||
                Math.Abs(dX) == 1 && dY == 1 && _game.CurrentPlayer == Player.White)
            result = MoveResult.Step;

        return result;
    }

    public void SelectSourceCell(Cell cell)
    {
        _game.history.AddCommand(new SelectSourceCellCommand(this, cell), true);
    }

    public void SelectTargetCell(Cell targetCell)
    {
        _game.history.AddCommand(new SelectTargetCellCommand(this, _game, targetCell), true);     
    }

    public MoveResult MakeMove(Cell startCell, Cell targetCell, List<CombatCommand> combatCommands, MoveCommand moveCommand)
    {
        MoveResult result = MoveResult.Prohibited;
        List<Combat> combats = GoalsFinder.GetCombatRouter().GetCombatsToCell(startCell, targetCell);

        if (combats.Count > 0)
        {
            result = MoveResult.Combat;
            combatCommands.Clear();
            foreach (Combat step in combats)
                combatCommands.Add(new CombatCommand(step, this, _game));
        }
        else if (GoalsFinder.StepCells.Contains(targetCell))
        {
            moveCommand = new MoveCommand(
            new Step(startCell, targetCell, startCell.figure.isKing), this, _game);

            result = combats.Count > 0 ? MoveResult.Combat : MoveResult.Step;
        }
        return result;
    }


    public Cell GetCellByPoint(Vector2 point)
    {
        try { return _grid.cells[(int)point.x, (int)point.y]; }
        catch { return null; }
    }

    public void Remove(Cell cell)
    {
        if (cell.figure != null)
            MonoBehaviour.Destroy(cell.figure.gameObject);
        cell.SetFigure(null);
    }

    public void ResetSourceCell()
    {
        SelectedCell = null;
        HighlightGoals(false);
    }


    private bool CanCheckerMove(Cell fromCell, Cell toCell)
    {
        if (!CanCellBeSource(fromCell)) return false;
        if (!CanCellBeTarget(toCell)) return false;
        return true;
    }

    public bool CheckAvailableGoals()
    {
        Player player = _game.MyPlayer;
        for (int y = 0; y < _grid.size; y++)
            for (int x = 0; x < _grid.size; x++)
            {
                Cell cell = _grid.cells[x, y];
                if (_game.IsMyTurn(cell))
                {
                    if (HasGoals(cell)) return true;
                }
            }
        return false;
    }

    public Cell GetEnemyCellBetween2Cells(Cell startCell, Cell endCell, bool isKing)
    {
        Cell enemyCell = null;
        Vector2 diagonal = GetDiagonal(startCell, endCell);
        if (isKing)
        {
            float dx = diagonal.x;
            float dy = diagonal.y;
            Vector2 addr = startCell.position;

            while (true)
            {
                addr = new Vector2(addr.x + dx, addr.y + dy);
                Cell cell = GetCellByPoint(addr);
                if (cell == null) break;
                if (cell.isProhibit() || addr == endCell.position) break;
                if (cell.State != State.Empty) break;
            }

            enemyCell = GetCellByPoint(addr);
        }
        else
        {
            Vector2 addr = new Vector2((startCell.position.x + endCell.position.x) / 2,
                                            (startCell.position.y + endCell.position.y) / 2);
            enemyCell = GetCellByPoint(addr);
        }

        return enemyCell;
    }

    public bool IsKingCell(Cell cell)
    {
        return (_game.CurrentPlayer == Player.White && cell.position.y == _grid.size - 1 ||
                    _game.CurrentPlayer == Player.Black && cell.position.y == 0);
    }

    public bool CanCellBeSource(Cell source)
    {
        return CanCellBeEnter(source) && (source.State != State.Empty) && _game.IsMyTurn(source);
    }

    public bool CanCellBeTarget(Cell target)
    {
        return CanCellBeEnter(target) && (target.State == State.Empty) && _game.IsMyTurn();
    }

    private bool CanCellBeEnter(Cell cell)
    {
        bool result = false;
        if (_game.Status != GameStatus.InProcess) return result;
        if (cell == null) return result;
        if (!Cell.isCorrectCell(cell.position)) return result;
        if (cell.State == State.Prohibited) return result;
        return true;
    }

    public Vector2 GetDiagonal(Cell startCell, Cell endCell)
    {
        Vector2 diagonal = new Vector2(
          Math.Sign(endCell.position.x - startCell.position.x),
          Math.Sign(endCell.position.y - startCell.position.y));

        return diagonal;
    }
}
