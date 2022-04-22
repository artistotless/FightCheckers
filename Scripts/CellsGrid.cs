using UnityEngine;

public class CellsGrid
{

    public Cell[,] cells { get; private set; }
    public int size { get; private set; }

    private CellsGridConfig _config;
    private GameObject _parentCells = new GameObject();
    private GameObject _parentFigures = new GameObject();

    private int[,] _boardTemplate =
           /*
           {
           {0,2,0,2,0,2,0,2},
           {2,0,2,0,2,0,2,0},
           {0,2,0,2,0,2,0,2},
           {0,0,0,0,0,0,0,0},
           {0,0,0,0,0,0,0,0},
           {1,0,1,0,1,0,1,0},
           {0,1,0,1,0,1,0,1},
           {1,0,1,0,1,0,1,0}
           };*/

           /*{
           //A B C D E F G H 
            {0,0,0,0,0,0,0,0}, //7
            {0,0,0,0,0,0,0,0}, //6
            {0,0,0,0,0,0,0,0}, //5
            {0,0,0,0,0,0,0,0}, //4
            {0,0,0,0,0,0,0,0}, //3
            {0,0,0,0,0,0,0,0}, //2
            {0,0,0,0,0,0,0,0}, //1
            {1,0,0,0,0,0,0,0}  //0
           //0 1 2 3 4 5 6 7
           };*/

           /*{
            //A B C D E F G H 
             {0,0,0,0,0,0,0,0}, //7
             {0,0,2,0,0,0,2,0}, //6
             {0,0,0,1,0,0,0,0}, //5
             {0,0,2,0,2,0,2,0}, //4
             {0,0,0,0,0,0,0,0}, //3
             {0,0,0,0,2,0,2,0}, //2
             {0,0,0,0,0,0,0,0}, //1
             {0,0,0,0,0,0,0,0}  //0
            //0 1 2 3 4 5 6 7
           };*/

           {
     //A B C D E F G H 
      {0,0,0,0,0,0,0,0}, //7
      {0,0,0,0,2,0,2,0}, //6
      {0,0,0,1,0,0,0,0}, //5
      {0,0,2,0,2,0,0,0}, //4
      {0,0,0,0,0,2,0,0}, //3
      {0,0,0,0,0,0,2,0}, //2
      {0,0,0,2,0,0,0,0}, //1
      {0,0,0,0,0,0,0,0}  //0
     //0 1 2 3 4 5 6 7
    };

    public CellsGrid(CellsGridConfig config)
    {
        _config = config;
        size = config.gridSize;
    }

    public void Generate()
    {
        cells = new Cell[size, size];
        _parentCells.name = "Board";
        _parentFigures.name = "Figures";

        ClearBoard();

        float offsetZ = -1;
        float offsetX = 1;
        float offsetY = -0.4f;

        for (int y = 0; y < _config.gridSize; y++)
        {
            offsetZ += 1f;
            offsetX = 0;

            for (int x = 0; x < _config.gridSize; x++)
            {
                Cell cell = MonoBehaviour.Instantiate<Cell>(_config.cellPrefab, _parentCells.transform, true);
                cell.name = $"[{x}:{y}] Cell";
                cell.tag = "Cell";
                cell.SetPose(new Vector2(x, y));
                cell.transform.position = new Vector3(cell.transform.position.x + offsetX, cell.transform.position.y + offsetY, cell.transform.position.z + offsetZ);
                cell.GetComponent<MeshRenderer>().sharedMaterial = (x + y) % 2 == 0 ? _config.blackCell : _config.whiteCell;
                cells[x, y] = cell;

                if (_boardTemplate[7 - y, x] != (int)FigureColor.Empty)
                {
                    FigureColor color = (FigureColor)_boardTemplate[7 - y, x];
                    Material colorMateial = color == FigureColor.White ? _config.whiteFigure : _config.blackFigure;
                    Figure figure = MonoBehaviour.Instantiate<Figure>(_config.figurePrefab, cells[x, y].transform.position, _config.figurePrefab.transform.rotation, _parentFigures.transform);
                    figure.SetColor(color, colorMateial);

                    if (color == FigureColor.Black)
                        figure.transform.Rotate(new Vector3(0, 180, 0));

                    cells[x, y].SetFigure(figure);
                }

                offsetX += 1f;
            }
        }
    }

    private void ClearBoard()
    {
        for (int i = 0; i < _parentCells.transform.childCount; i++)
            GameObject.DestroyImmediate(_parentCells.transform.GetChild(i).gameObject);
    }

    public Cell[,] GetMap()
    {
        return cells;
    }

}

