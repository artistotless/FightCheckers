using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public enum Player
{
    White,
    Black
}

public enum GameStatus
{
    NotStarted,   // игра не начата
    InProcess,   // игра идёт
    WhiteWon,  // белые выиграли
    BlackWon,  // чёрные выиграли
    Draw    // ничья
}

public enum PlayMode
{
    Game,       // игра с компьютером
    NetGame,    // игра по сети
    SelfGame,   // игра с самим собой
    Collocation // расстановка фишек
}

public class Game : MonoBehaviour
{

    //public Text scoreUi;
    public History history;
    public Vector2 lastDiagonal;
    public Cell SelectedCell;
    public Figure SelectedFigure;
    public CellsGridConfig gridConfig;
    public Board board;

    public int WhiteScore { get { return _whiteScore; } set { _whiteScore = value; Score = ""; } }
    public int BlackScore { get { return _blackScore; } set { _blackScore = value; Score = ""; } }
    private int _whiteScore;
    private int _blackScore;

    private string Score
    {
        get { return string.Empty; }
        set
        {
            //scoreUi.text = $"{WhiteScore.ToString()} : {BlackScore.ToString()}";
        }
    }
    internal bool Direction { get { return CurrentPlayer == Player.White ? false : true; } set { } }
    public Player CurrentPlayer = Player.White;
    public Player MyPlayer;
    public GameStatus Status;
    public PlayMode Mode;

    private GameObject _endGamePanel;
    private Text winnerText;

    public void SwitchCurrentPlayer()
    {
        CurrentPlayer = CurrentPlayer == Player.White ? Player.Black : Player.White;
        if (Mode == PlayMode.SelfGame)
            MyPlayer = CurrentPlayer;
    }

    public bool IsMyTurn(Cell selectedCell = null)
    {
        if (selectedCell != null)
        {
            return ((CurrentPlayer == Player.White && selectedCell.State == State.White ||
                CurrentPlayer == Player.Black && selectedCell.State == State.Black) && CurrentPlayer == MyPlayer);
        }
        return CurrentPlayer == MyPlayer;
    }

    public void UpdateScore()
    {
        if (CurrentPlayer == Player.Black)
            BlackScore++;
        else
            WhiteScore++;
    }

    public void CheckWin()
    {
        int[] figuresCount = { 14, 14 };
        //int[] figuresCount = board.GetFiguresCount();

        Status = WhiteScore == 12
            ? GameStatus.WhiteWon
            : BlackScore == 12
                 ? GameStatus.BlackWon
                 : GameStatus.InProcess;

        if (figuresCount[0] == 0)
            Status = GameStatus.BlackWon;
        else if (figuresCount[1] == 0)
            Status = GameStatus.WhiteWon;

        if (Status == GameStatus.NotStarted || Status == GameStatus.InProcess) return;
        _endGamePanel.SetActive(true);
        winnerText.text = Status.ToString();

    }

    void Awake()
    {
        Application.targetFrameRate = 300;
        this.CurrentPlayer = Player.White;
        this.MyPlayer = Player.White;
        this.Status = GameStatus.InProcess;
        this.Mode = PlayMode.SelfGame;


        DOTween.Init();
    }
    public void FixedUpdate()
    {
        SelectedCell = board.SelectedCell;
        lastDiagonal = board.lastDiagonalMove.HasValue ? board.lastDiagonalMove.Value : lastDiagonal;
        SelectedFigure = board.SelectedCell == null ? null : board.SelectedCell.figure;
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    void Start()
    {
        MouseEventService.Instance.cellClicked += SelectCell;


        CellsGrid grid = new CellsGrid(gridConfig);
        board = new Board(grid, this);
        board.checkerMoveEvent += OnCheckerMoved;

        MouseEventService.Instance.nextStep += () => { history.StepNext(); };
        MouseEventService.Instance.prevStep += () => { history.StepPrevious(); };
    }

    public void OnCheckerMoved(Cell endCell)
    {

    }

    public void SelectCell(Cell cell)
    {
        if (cell == null) return;
        // Выбрали поле впервые - устанавливаем board.Selected
        if (board.SelectedCell == null)
            board.SelectSourceCell(cell);

        // Выбираем целевое поле
        else
        {
            // Выбрано тоже самое поле 2 раза - обнуляем board.Selected
            if (board.SelectedCell == cell)
            {
                board.ResetSourceCell();
                return;
            }
            // Выбрано другое не пустое поле - переназначаем board.Selected
            if (cell.figure != null)
                if (cell.figure.color == board.SelectedCell.figure.color)
                {
                    board.ResetSourceCell();
                    board.SelectSourceCell(cell);
                    return;
                }
            // Выбрано пустое поле - делаем ход
            board.SelectTargetCell(cell);
        }
    }
}

