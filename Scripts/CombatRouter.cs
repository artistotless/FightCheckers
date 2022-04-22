using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

[Serializable]
public struct Route
{
    public List<Combat> Combats { get; private set; }
    public Cell endCell;

    public Route(List<Combat> combats)
    {
        Combats = combats;
        endCell = combats[combats.Count - 1].endCell;
    }

    public void Print()
    {
        foreach (Combat way in Combats)
            Debug.Log($"[ {way.startCell.position.x} , {way.startCell.position.y} ] -> [ {way.endCell.position.x} , {way.endCell.position.y} ] [ {(way.isKing ? "king" : "checker")} ]");
    }
}

[Serializable]
public struct Combat
{
    public Cell enemyCell;
    public Cell endCell;
    public Cell startCell;
    public bool isKing;
}

[Serializable]
public struct Step
{
    public Cell startCell;
    public Cell endCell;
    public bool isKing;

    public Step(Cell startCell, Cell endCell, bool isKing)
    {
        this.startCell = startCell;
        this.endCell = endCell;
        this.isKing = isKing;
    }
}

public class CombatRouter
{
    public List<Combat> Combats { get; } = new List<Combat>();
    public List<Route> Routes { get; } = new List<Route>();

    public void AddCombat(Combat c) { Combats.Add(c); }

    public List<Combat> GetCombatsToCell(Cell from, Cell to)
    {
        foreach (Route r in Routes)
        {
            if (r.endCell == to)
                return r.Combats;
            int fromIndex = r.Combats.FindIndex(x => x.startCell == from);
            int toIndex = r.Combats.FindIndex(x => x.endCell == to);
            if (fromIndex != -1 && toIndex != -1)
            {
                List<Combat> steps = r.Combats.GetRange(fromIndex, toIndex + 1);
                return steps;
            }
        }

        return new List<Combat>();
    }

    public void Print()
    {
        foreach (Route route in Routes)
            route.Print();
    }

    public bool isEndCell(Cell cell)
    {
        if (Routes.Count == 0) return false;
        return Routes.Where(x => x.endCell == cell).Count() > 0;
    }

    public void ComputeRoutes()
    {
        Routes.Clear();
        List<Combat> tempSteps = new List<Combat>();

        for (int i = 0; i < Combats.Count; i++)
        {
            tempSteps.Add(Combats[i]);
            if (i == Combats.Count - 1)
            {
                Routes.Add(new Route(new List<Combat>(tempSteps)));
                break;
            }

            if (Combats[i + 1].startCell != Combats[i].endCell)
                Routes.Add(new Route(new List<Combat>(tempSteps)));
            else
                continue;

            if (Combats[i + 1].startCell == Combats[i].startCell)
                tempSteps.RemoveAt(tempSteps.Count - 1);
            else
                tempSteps.Clear();
        }

        tempSteps.Clear();
        Combats.Clear();
    }
}
