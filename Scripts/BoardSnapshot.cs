using System;
using UnityEngine;

public class BoardSnapshot : ISnapshot<BoardSnapshotData>
{

    private BoardSnapshotData _data;

    public BoardSnapshot(BoardSnapshotData data)
    {
        _data = data;
    }

    public BoardSnapshotData GetData()
    {
        return _data;
    }
}

public struct BoardSnapshotData
{
    public GoalsFinder goalsFinder;
    public Vector2? lastDiagonalMove;
    public Cell Selected;

    public BoardSnapshotData(GoalsFinder goalsFinder, Vector2? lastDiagonalMove, Cell selected)
    {
        this.goalsFinder = (GoalsFinder)((ICloneable)goalsFinder).Clone();
        this.lastDiagonalMove = lastDiagonalMove;
        Selected = selected;
    }
}