using UnityEngine;

[CreateAssetMenu(fileName = "CellsGridConfig", menuName = "ScriptableObjects/Configs", order = 1)]
public class CellsGridConfig:ScriptableObject
{
    public int gridSize = 8;
    public Cell cellPrefab;
    public Figure figurePrefab;
    public Material whiteCell, blackCell;
    public Material whiteFigure, blackFigure;
}

