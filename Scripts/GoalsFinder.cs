using System.Collections.Generic;
using UnityEngine;
using System;

public enum SearchType { NonKingSteps, KingSteps, NonKingCombats, KingCombats }
public class GoalsFinder : ICloneable
{
    public List<Cell> CombatCells;
    public List<Cell> StepCells;
    public bool hasGoals { get { return CombatCells.Count > 0 || StepCells.Count > 0; } private set { } }

    public Board _board { get; private set; }
    public CombatRouter _router { get; private set; }
    public List<Cell> _lastHighLightedCells { get; private set; }
    public GoalDirection[] _arrayDirections { get; private set; }
        = { GoalDirection.TopRight, GoalDirection.BottomLeft, GoalDirection.BottomRight, GoalDirection.TopLeft };

    public GoalsFinder() { }
    public GoalsFinder(Board board)
    {
        CombatCells = new List<Cell>();
        StepCells = new List<Cell>();
        _board = board;
        _router = new CombatRouter();
        _lastHighLightedCells = new List<Cell>();
    }

    public CombatRouter GetCombatRouter()
    {
        return _router;
    }

    public void ClearData()
    {
        CombatCells.Clear();
        StepCells.Clear();
    }

    public void HighlightGoals(bool enabled = true)
    {
        List<Cell> targetCollection = !enabled ? _lastHighLightedCells : (CombatCells.Count > 0 ? CombatCells : StepCells);
        foreach (Cell cell in targetCollection)
            cell.Highlight(enabled);
        _lastHighLightedCells = targetCollection;
    }

    public void Find(Cell selectedCell, bool isKing)
    {
        ClearData();
        CombatCells.AddRange(FindCombats(selectedCell, isKing));
        if (CombatCells.Count > 0)
            RecursiveFindCombats(CombatCells, null, isKing);
        else
            StepCells.AddRange(FindSteps(selectedCell, isKing));
        _router.ComputeRoutes();
    }

    public void FindAndFillResults(Cell selectedCell, bool isKing, List<Cell> _Combats, List<Cell> _steps)
    {
        _Combats.AddRange(FindCombats(selectedCell, isKing));
        if (CombatCells.Count > 0) return;
        _steps.AddRange(FindSteps(selectedCell, isKing));
    }

    private void RecursiveFindCombats(List<Cell> cells, Cell _startCell = null, bool? isKing = null)
    {
        bool _isKing = !isKing.HasValue ? _board.SelectedCell.figure.isKing : isKing.Value;
        Cell startCell = _startCell == null ? _board.SelectedCell : _startCell;

        Queue<Cell> copyBattleCells = new Queue<Cell>(cells);

        while (copyBattleCells.Count > 0)
        {
            Cell targetCell = copyBattleCells.Dequeue();
            Combat step = new Combat() { startCell = startCell, endCell = targetCell, enemyCell = _board.GetEnemyCellBetween2Cells(startCell, targetCell, _isKing), isKing = _isKing };
            _router.AddCombat(step);

            if (_board.IsKingCell(targetCell))
                _isKing = true;

            List<Cell> CombatsFound = FindCombats(targetCell, _isKing, startCell);
            if (CombatsFound.Count > 0)
                RecursiveFindCombats(CombatsFound, targetCell, _isKing);

            _isKing = isKing.HasValue ? isKing.Value : false;
        }
    }

    private List<Cell> FindCombats(Cell targetCell, bool isKing, Cell startCell = null)
    {
        List<Cell> Combats = isKing ? FindKingCombats(targetCell, startCell) : FindNonKingCombats(targetCell, startCell);
        DeleteWrongDiagonalCombats(Combats, targetCell, startCell);
        return Combats;
    }

    private void DeleteWrongDiagonalCombats(List<Cell> Combats, Cell targetCell, Cell startCell = null)
    {
        Vector2 diagonal = (startCell is null) ? (_board.lastDiagonalMove.HasValue ? _board.lastDiagonalMove.Value : _board.GetDiagonal(_board.SelectedCell, targetCell))
            : _board.GetDiagonal(startCell, targetCell);
        if (diagonal == Vector2.zero) return;
        for (int i = Combats.Count - 1; i >= 0; i--)
        {
            Vector2 _diagonal = _board.GetDiagonal(targetCell, Combats[i]);
            if (_diagonal.x != diagonal.x && _diagonal.y != diagonal.y)
                Combats.RemoveAt(i);
        }
    }

    private List<Cell> FindSteps(Cell selectedCell, bool isKing)
    {
        return isKing ? FindKingSteps(selectedCell) : FindNonKingSteps(selectedCell);
    }

    private List<Cell> FindNonKingSteps(Cell targetCell, Cell startCell = null, List<Vector2> _offsetList = null)
    {
        List<Vector2> offsetList = _offsetList == null ? GetOffsetList(_arrayDirections, SearchType.NonKingSteps) : _offsetList;
        List<Cell> result = new List<Cell>();
        foreach (Vector2 offset in offsetList)
        {
            Vector2 addr = new Vector2(targetCell.position.x + offset.x, targetCell.position.y + offset.y);
            Cell nextCell = _board.GetCellByPoint(addr);
            MoveResult check = _board.CheckMove(targetCell, nextCell);

            if (check != MoveResult.Prohibited)
                result.Add(nextCell);
        }

        return result;
    }

    private List<Cell> FindNonKingCombats(Cell targetCell, Cell startCell = null)
    {
        return FindNonKingSteps(targetCell, startCell, GetOffsetList(_arrayDirections, SearchType.NonKingCombats));
    }

    private List<Cell> FindKingSteps(Cell selectedCell)
    {
        List<Vector2> offsetList = GetOffsetList(_arrayDirections, SearchType.KingSteps);
        List<Cell> result = new List<Cell>();
        foreach (Vector2 offset in offsetList)
        {
            Vector2 addr = selectedCell.position;
            while (true)
            {
                addr = new Vector2(addr.x + offset.x, addr.y + offset.y);
                Cell cell = _board.GetCellByPoint(addr);
                if (!Cell.isCorrectCell(cell) || cell.isProhibit() || selectedCell.State == cell.State) break;
                if (cell.State == State.Empty)
                    result.Add(cell);
            }
        }
        return result;
    }

    private List<Cell> FindKingCombats(Cell targetCell, Cell startCell = null)
    {
        List<Vector2> offsetList = GetOffsetList(_arrayDirections, SearchType.KingSteps);
        List<Cell> result = new List<Cell>();

        bool CheckCellForProhibitState(Cell cell)
        {
            if (cell is null) return true;
            return (!Cell.isCorrectCell(cell) || cell.isProhibit());
        }

        foreach (Vector2 offset in offsetList)
        {
            bool combat = false;
            Vector2 addr = targetCell.position;
            while (true)
            {
                addr = new Vector2(addr.x + offset.x, addr.y + offset.y);
                Cell cell = _board.GetCellByPoint(addr);
                if (CheckCellForProhibitState(cell)) break;

                if (cell.State == State.Empty)
                {
                    if (combat)
                        result.Add(cell);
                }

                else if (cell.State != targetCell.State)
                {
                    addr = new Vector2(addr.x + offset.x, addr.y + offset.y);
                    cell = _board.GetCellByPoint(addr);
                    if (CheckCellForProhibitState(cell) || cell.State != State.Empty || combat) break;
                    result.Add(cell);
                    combat = true;
                }
                else break;
            }
        }
        return result;
    }

    private List<Vector2> GetOffsetList(GoalDirection[] directionArray, SearchType type)
    {
        int multiple = type == SearchType.NonKingCombats ? 2 : 1;
        List<Vector2> offsetList = new List<Vector2>();

        foreach (GoalDirection direction in directionArray)
            switch (direction)
            {
                case GoalDirection.TopRight: offsetList.Add(new Vector2(+1 * multiple, +1 * multiple)); break;
                case GoalDirection.TopLeft: offsetList.Add(new Vector2(-1 * multiple, +1 * multiple)); break;
                case GoalDirection.BottomRight: offsetList.Add(new Vector2(+1 * multiple, -1 * multiple)); break;
                case GoalDirection.BottomLeft: offsetList.Add(new Vector2(-1 * multiple, -1 * multiple)); break;
            }
        return offsetList;
    }

    public object Clone()
    {
        return new GoalsFinder()
        {
            CombatCells = new List<Cell>(this.CombatCells),
            StepCells = new List<Cell>(this.StepCells),
            hasGoals = this.hasGoals,
            _lastHighLightedCells = new List<Cell>(this._lastHighLightedCells),
            _router = this._router,
            _board = this._board,
            _arrayDirections = this._arrayDirections
        };
    }
}


