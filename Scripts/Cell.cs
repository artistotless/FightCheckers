using UnityEngine;

public enum State
{
    Prohibited = 3,
    Empty = 0,
    White = 2,
    Black = 1
}

public class Cell : MonoBehaviour
{
    [SerializeField]
    public Vector2 position;
    public Material hightlightMaterial;
    public Figure figure;

    private MeshRenderer _meshRenderer;
    private Material _defaultMaterial;
    private State _state;
    public State State
    {
        get
        {
            if (figure == null)
                return State.Empty;
            else
                return figure.color == FigureColor.Black ? State.Black : State.White;
        }
        set { _state = value; }
    }

    public void SetPose(Vector2 position)
    {
        this.position = position;
        if ((position.x + position.y) % 2 == 0) { gameObject.AddComponent<BoxCollider>(); }
    }

    public bool isProhibit()
    {
        return ((position.x + position.y) % 2 != 0);
    }

    public static bool isCorrectCell(Vector2 position)
    {
        return (position.x >= 0 && position.x < 8 && position.y >= 0 && position.y < 8);
    }

    public static bool isCorrectCell(Cell cell)
    {
        if (cell == null) return false;
        return isCorrectCell(cell.position);
    }

    public void Highlight(bool enabled=true)
    {
        _meshRenderer.material = enabled ? hightlightMaterial : _defaultMaterial;
    }

    public bool IsHighlighted()
    {
        return _meshRenderer.material == hightlightMaterial;
    }

    public void SetFigure(Figure figure) { this.figure = figure; }

    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _defaultMaterial = _meshRenderer.material;
    }
}
