//using System;
//using UnityEngine;
//using DG.Tweening;
//using System.Collections;
//using System.Collections.Generic;

///// <summary>
///// Результат хода
///// </summary>
//public enum MoveResult
//{
//    Prohibited,                 // запрещено
//    SuccessfullMove,            // разрешённый простой ход
//    SuccessfullCombat           // разрешённое взятие шашки противника
//}

//public enum GoalDirection
//{
//    BottomLeft,
//    N,
//    BottomRight,
//    E,
//    TopRight,
//    S,
//    TopLeft,
//    W
//}

//public class Board : MonoBehaviour
//{
//    public float durationGizmosLine = 5.0f;
//    private readonly Hashtable _fields = new Hashtable();
//    private Cell _selected;
//    private Vector2? _lastDiagonalMove = null;
//    private bool _lastDirection;
//    [SerializeField] private List<Route> _routes = new List<Route>();
//    public int _movedCount = 0;
//    private delegate void BattleFoundAction(List<Cell> battleCells, Cell _startCell = null, bool? isKing = null);
//    private BattleFoundAction _BattleFoundEvent;



//    List<Cell> _steps = new List<Cell>();
//    [SerializeField]
//    public List<Cell> _battles = new List<Cell>();
//    private Router _router;
//    public bool setWhiteKing = false;

//    private CellsGrid _grid;
//    private Game _game;

//    private void Awake()
//    {
//        _BattleFoundEvent += OnBattleFound;
//    }

//    public void Init(CellsGrid grid, Game game)
//    {
//        _grid = grid;
//        _game = game;
//        _router = new Router();
//        _grid.Generate();

//    }

//    /// <summary>
//    /// Текущая ячейка
//    /// </summary>
//    public Cell Selected
//    {
//        get { return _selected; }
//        private set { _selected = value; }
//    }


//    void HighLightGoalCells()
//    {
//        foreach (var step in _steps)
//            step.Highlight(true);
//        foreach (var battle in _battles)
//            battle.Highlight(true);
//    }

//    /// <summary>
//    /// При текущем направлении игры проверяем, если у стороны "бьющие" фишки
//    /// </summary>
//    /// <returns></returns>
//    public bool HasAnyCombat()
//    {
//        var result = false;
//        for (int y = 0; y < _grid.size; y++)
//            for (int x = 0; x < _grid.size; x++)
//            {
//                var cell = _grid.cells[x, y];

//                var cellState = cell == null ? State.Prohibited : cell.State;
//                if (cellState == State.Empty || cellState == State.Prohibited) continue;
//                if ((_game.Direction && cellState == State.Black ||
//                    !_game.Direction && cellState == State.White) && HasCombat(cell))
//                    return true;
//            }

//        return result;
//    }

//    /// <summary>
//    /// Фишка на этой позиции может произвести "бой"
//    /// </summary>
//    /// <param name="pos"></param>
//    /// <returns></returns>
//    public bool HasCombat(Cell cell)
//    {
//        var result = false;

//        if (cell.isEmpty()) return result;
//        // запрет хода для фишек не в свою очередь
//        if (_game.Direction && cell.State != State.Black ||
//            !_game.Direction && cell.State != State.White) { Debug.Log("Не ваша очередь (HasCombat)"); return result; }
//        return HasGoals(cell, true);
//    }

//    public void ClearHighlights()
//    {
//        for (int y = 0; y < 8; y++)
//        {
//            for (int x = 0; x < 8; x++)
//            {
//                if (_grid.cells[x, y] != null)
//                    _grid.cells[x, y].Highlight(false);
//            }
//        }
//    }

//    /// <summary>
//    /// Проверка возможности "хода" фишки
//    /// </summary>
//    /// <param name="startPos">Начальная позиция</param>
//    /// <param name="endPos">Конечная позиция</param>
//    /// <param name="direction">Направление проверки: false - фишки идут вверх, true - фишки идут вниз</param>
//    /// <returns>Результат хода</returns>
//    public MoveResult CheckMove(Vector2 startPos, Vector2 endPos, bool? king = null, bool hypothetically = false, State? cellState = null)
//    {
//        var result = MoveResult.Prohibited;
//        if (endPos.x < 0 || endPos.x >= 8 || endPos.y < 0 || endPos.y >= 8)
//            return result;
//        if (!hypothetically)
//            if (_grid.cells[(int)startPos.x, (int)startPos.y].isEmpty() || _grid.cells[(int)endPos.x, (int)endPos.y].isEmpty()) return result;

//        var startCellState = cellState != null ? (State)cellState : (_grid.cells[(int)startPos.x, (int)startPos.y]).State;
//        if (startCellState != State.Empty)
//        {
//            // запрет хода для фишек не в свою очередь
//            if (_game.Direction && startCellState != State.Black ||
//                !_game.Direction && startCellState != State.White) { Debug.Log("Не ваша очередь (CheckMove)"); return result; }
//        }
//        var dX = endPos.x - startPos.x;
//        var dY = endPos.y - startPos.y;

//        var startCellIsKing = king != null ? (bool)king : (_grid.cells[(int)startPos.x, (int)startPos.y]).currentFigure.isKing;

//        var targetCellState = (_grid.cells[(int)endPos.x, (int)endPos.y]).State;

//        if (targetCellState == State.Empty)
//        {
//            // проверка "боя"
//            if (Math.Abs(dX) == 2 && Math.Abs(dY) == 2)
//            {
//                // поиск "промежуточной" ячейки
//                var victimPos = new Vector2((startPos.x + endPos.x) / 2,
//                                            (startPos.y + endPos.y) / 2);
//                var victimCellState = (_grid.cells[(int)victimPos.x, (int)victimPos.y]).State;
//                // снимаем только фишку противника
//                result = targetCellState != victimCellState && startCellState != victimCellState
//                            ? MoveResult.SuccessfullCombat : result;
//            }
//            // проверка "хода"
//            if (Math.Abs(dX) == 1 && dY == -1 && _game.Direction ||
//                    Math.Abs(dX) == 1 && dY == 1 && !_game.Direction ||
//                    // дамка "ходит" во все стороны
//                    Math.Abs(dX) == 1 && Math.Abs(dY) == 1 && startCellIsKing)
//                result = MoveResult.SuccessfullMove;
//        }
//        return result;
//    }

//    /// <summary>
//    /// Делаем "ход"
//    /// </summary>
//    /// <param name="startCell">Начальная позиция</param>
//    /// <param name="endCell">Конечная позиция</param>
//    /// <returns>результат хода</returns>
//    public MoveResult MakeMove(Cell startCell, Cell endCell)
//    {
//        var moveResult = _steps.Contains(endCell) ? MoveResult.SuccessfullMove : MoveResult.Prohibited;
//        if (moveResult == MoveResult.SuccessfullMove)
//        {
//            if (!HasCombat(startCell))
//                Move(startCell, endCell);
//            else
//            {
//                Debug.Log("Обязан рубить фишку противника");
//                return MoveResult.Prohibited; // обязан брать шашку противника
//            }
//        }
//        else
//        {
//            //TODO: доделать поочередный ход, убрать вообще _battles, вместо него уже есть Router.Routes
//            moveResult = _battles.Contains(endCell) ? MoveResult.SuccessfullCombat : _router.GetStepsToCell(endCell).Count > 0 ? MoveResult.SuccessfullCombat : MoveResult.Prohibited;
//            if (moveResult == MoveResult.SuccessfullCombat)
//            {

//                // снятие фишек противника
//                var diagonal = GetDiagonal(startCell, endCell);
//                var dx = diagonal.x;
//                var dy = diagonal.y;
//                var addr = startCell.position;
//                Cell enemyCell = null;
//                while (true)
//                {
//                    addr = new Vector2(addr.x + dx, addr.y + dy);
//                    enemyCell = GetCellByPoint(addr);
//                    if (enemyCell.isEmpty() || addr == endCell.position) break;
//                    //Move(startCell, enemyCell);
//                    Remove(GetCellByPoint(addr));
//                }
//                Move(startCell, endCell);
//            }
//        }
//        return moveResult;
//    }


//    public Cell GetCellByPoint(Vector2 point)
//    {
//        try
//        {
//            return _grid.cells[(int)point.x, (int)point.y];
//        }
//        catch { return null; }
//    }


//    /// <summary>
//    /// Переносим фишку на доске (вспомогательный метод)
//    /// </summary>
//    /// <param name="startPos">Начальная позиция</param>
//    /// <param name="endPos">Конечная позиция</param>
//    private void Move(Cell startCell, Cell endCell)
//    {
//        startCell.currentFigure.transform.DOMove(endCell.transform.position, 1.3f);
//        //StartCoroutine(startCell.currentFigure.GetComponent<RMCharacterController>().MoveToTarget(endCell.transform.position));
//        endCell.SetFigure(startCell.currentFigure);
//        startCell.SetFigure(null);
//    }

//    /// <summary>
//    /// Убираем фишку с доски (вспомогательный метод)
//    /// </summary>
//    /// <param name="pos">Позиция фишки</param>
//    private void Remove(Cell cell)
//    {
//        if (cell.currentFigure != null)
//            Destroy(cell.currentFigure.gameObject);
//        cell.currentFigure = null;
//        //cell.State = State.Empty;
//        //cell.currentFigure.isKing = false;
//    }



//    public event Action<bool, Vector2, Vector2, MoveResult, int> CheckerMoved = delegate { };

//    /// <summary>
//    /// Обработка события перемещения фишки
//    /// </summary>
//    /// <param name="direction">Текущее направление игры: false - ходят белые</param>
//    /// <param name="startPos">Начальное положение</param>
//    /// <param name="endPos">Конечное положение</param>
//    /// <param name="moveResult">Результат хода</param>
//    private void OnCheckerMoved(bool direction, Vector2 startPos, Vector2 endPos, MoveResult moveResult, int stepCount)
//    {
//        CheckerMoved(direction, startPos, endPos, moveResult, stepCount);
//    }

//    public event Action ActivePlayerChanged = delegate { };

//    private void OnActivePlayerChanged()
//    {
//        _game.CurrentPlayer = _game.CurrentPlayer == Player.White ? Player.Black : Player.White;
//        ActivePlayerChanged();
//    }

//    /// <summary>
//    /// Выбираем фишку для начала хода
//    /// </summary>
//    /// <param name="cell"></param>
//    public void SelectSourceCell(Cell cell)
//    {
//        if (_game.WinPlayer != WinPlayer.Game) return;
//        if (Cell.isCorrectCell(cell.position) && cell.State != State.Prohibited)
//        {
//            if (_game.Mode == PlayMode.Game || _game.Mode == PlayMode.NetGame)
//            {
//                if (_game.CurrentPlayer == Player.Black && !_game.Direction ||
//                    _game.CurrentPlayer == Player.White && _game.Direction) return;
//            }
//            // если ячейка не пустая, но не может быть выбрана
//            if (cell.State != State.Empty && !CanCellEnter(cell))
//                return;
//            // не должно быть выбранной ячейки с фишкой
//            if (Selected == null)
//                // можем выбирать фишки только цвета игрока
//                SetSelectedCell(cell);
//        }
//    }

//    public int[] GetFiguresCount()
//    {
//        // TODO: слишком сложный метод, упростить его и избавиться от итерации массива
//        int[] count = { 0, 0 };

//        for (int y = 0; y < _grid.size; y++)
//            for (int x = 0; x < _grid.size; x++)
//                if (_grid.cells[x, y] != null)
//                    if (_grid.cells[x, y].currentFigure != null)
//                    {
//                        if (_grid.cells[x, y].currentFigure.color == FigureColor.White)
//                            count[0] += 1;
//                        else if (_grid.cells[x, y].currentFigure.color == FigureColor.Black)
//                            count[1] += 1;
//                    }
//        return count;
//    }

//    /// <summary>
//    /// Выбор целевой ячейки для хода или боя
//    /// </summary>
//    /// <param name="targetCell"></param>
//    public void SelectTargetCell(Cell targetCell)
//    {
//        if (_game.WinPlayer != WinPlayer.Game) return;
//        if (Cell.isCorrectCell(targetCell.position) && targetCell.State != State.Prohibited)
//        {
//            if (_game.Mode == PlayMode.Game || _game.Mode == PlayMode.NetGame)
//            {
//                if (_game.CurrentPlayer == Player.Black && !_game.Direction ||
//               _game.CurrentPlayer == Player.White && _game.Direction) return;
//            }
//            if (Selected != null && targetCell.State == State.Empty) // ранее была выбрана фишка и выбрана пустая клетка
//                                                                     // пробуем делать ход
//            {
//                var startPos = Selected.position;
//                var endPos = targetCell.position;
//                var startCell = _grid.cells[(int)startPos.x, (int)startPos.y];
//                var endCell = _grid.cells[(int)endPos.x, (int)endPos.y];
//                var moveResult = MakeMove(startCell, endCell);
//                var lastSelected = Selected;
//                Selected = null;  // после хода сбрасываем текущую выбранную фишку
//                if (moveResult == MoveResult.Prohibited) return;

//                _lastDiagonalMove = GetDiagonal(lastSelected, targetCell);
//                _lastDirection = _game.Direction;
//                // подсчёт очков
//                if (moveResult == MoveResult.SuccessfullCombat)
//                {
//                    if (_game.Direction)
//                        _game.BlackScore++;
//                    else
//                        _game.WhiteScore++;
//                }
//                _game.CheckWin();
//                // считаем количество непрерывных ходов одной стороной
//                _movedCount++;
//                // определение дамки
//                if ((!_game.Direction && targetCell.position.y == _grid.size - 1 ||
//                     _game.Direction && targetCell.position.y == 0)) targetCell.currentFigure.isKing = true;
//                var hasCombat = HasCombat(targetCell); // есть ли в этой позиции возможность боя
//                                                       // запоминаем очередь хода перед возможной сменой
//                var lastDirection = _game.Direction;
//                // или был бой и далее нет возможности боя
//                if (moveResult == MoveResult.SuccessfullCombat && !hasCombat ||
//                    moveResult == MoveResult.SuccessfullMove)
//                {
//                    // сообщаем о перемещении фишки
//                    OnCheckerMoved(lastDirection, startPos, endPos, moveResult, _movedCount);
//                    // сбрасываем количество непрерывных ходов одной стороной
//                    _movedCount = 0;
//                    // передача очерёдности хода
//                    _game.Direction = !_game.Direction;
//                    OnActivePlayerChanged();
//                    _game.CheckWin();
//                    if (_game.WinPlayer == WinPlayer.None)
//                        CheckAvailableGoals();
//                    return;
//                }
//                else if (moveResult == MoveResult.SuccessfullCombat && hasCombat)
//                    // выбрана фишка для продолжения боя
//                    SetSelectedCell(targetCell);
//                // сообщаем о перемещении фишки
//                OnCheckerMoved(lastDirection, startPos, endPos, moveResult, _movedCount);
//            }

//        }
//    }

//    private bool CheckAvailableGoals()
//    {
//        var player = _game.CurrentPlayer;
//        for (int y = 0; y < _grid.size; y++)
//            for (int x = 0; x < _grid.size; x++)
//            {
//                var cell = _grid.cells[x, y];
//                if (player == Player.White && cell.State == State.White ||
//                    player == Player.Black && cell.State == State.Black)
//                {
//                    if (HasGoals(cell)) return true;
//                }
//            }
//        return false;
//    }

//    /// <summary>
//    /// Выбираем только "игровые" клетки, по которым фишки двигаются
//    /// </summary>
//    /// <param name="cell">Выбранная ячейка</param>
//    private void SetSelectedCell(Cell cell)
//    {
//        if (cell.State == State.Black || cell.State == State.White)
//        {
//            Selected = cell;

//            _steps.Clear();
//            _battles.Clear();

//            Debug.ClearDeveloperConsole();
//            ClearHighlights();
//            if (_selected != null)
//            {
//                _router.Clear();
//                FillGoalCells(Selected);
//                _router.ComputeRoutes();
//                _routes = _router.Routes;
//                _router.Print();
//            }
//            cell.transform.DOPunchScale(new Vector3(0, 0, 0.01f), 0.4f);
//        }
//    }

//    /// <summary>
//    /// Проверка возможности выбрать указанную ячейку для начала "хода"
//    /// </summary>
//    /// <param name="cell">Целевая ячейка</param>
//    /// <returns>true - выбор возможен</returns>
//    public bool CanCellEnter(Cell cell)
//    {
//        // пустую ячейку сделать текущей не можем
//        if (cell.State == State.Empty) return false;
//        // "ходят" чёрные и пытаемся выбрать белую фишку
//        // "ходят" белые и пытаемся выбрать чёрную фишку
//        if (_game.Direction && cell.State == State.White ||
//            !_game.Direction && cell.State == State.Black) return false;
//        // у фишки нет ходов
//        if (!HasGoals(cell)) return false;
//        // если по очереди некоторые фишки могут "ударить", но эта фишка "ударить" не может
//        if (HasAnyCombat() && !HasCombat(cell)) return false;
//        return true;
//    }

//    void OnBattleFound(List<Cell> cells, Cell _startCell = null, bool? isKing = null)
//    {
//        if (Selected == null) return;
//        var copy = new Queue<Cell>();
//        foreach (var item in cells)
//            copy.Enqueue(item);
//        bool _isKing = !isKing.HasValue ? Selected.currentFigure.isKing : isKing.Value;
//        var startCell = _startCell == null ? Selected : _startCell;

//        while (copy.Count > 0)
//        {
//            var cell = copy.Dequeue();
//            if (startCell.position == new Vector2(1, 5) && cell.position == new Vector2(3, 3)) { }

//            var way = new Step() { currentCell = startCell, nextCell = cell, isKing = _isKing };
//            _router.AddStep(way);

//            if ((!_game.Direction && cell.position.y == _grid.size - 1 ||
//                    _game.Direction && cell.position.y == 0))
//                _isKing = true;

//            var battlesFound = FindMoreBattles(startCell, cell, _isKing);
//            if (battlesFound != null)
//                OnBattleFound(battlesFound, cell, _isKing);

//            _isKing = isKing.HasValue ? isKing.Value : false;
//        }
//    }


//    Vector2 GetDiagonal(Cell startCell, Cell endCell)
//    {
//        var diagonal = new Vector2(
//          Math.Sign(endCell.position.x - startCell.position.x),
//          Math.Sign(endCell.position.y - startCell.position.y));

//        return diagonal;
//    }

//    /// <summary>
//    /// Есть ли у фишки вообще ходы?
//    /// </summary>
//    /// <param name="cell"></param>
//    /// <returns></returns>
//    private bool HasGoals(Cell cell, bool combat = false)
//    {
//        var steps = new List<Cell>();
//        var battles = new List<Cell>();
//        FillGoalCells(cell, steps, battles);

//        if (_lastDiagonalMove != null && combat && _lastDirection == _game.Direction)
//        {
//            List<Cell> _battles = new List<Cell>();
//            foreach (var item in battles)
//            {
//                var _diagonal = GetDiagonal(cell, item);
//                if ((_diagonal.x != _lastDiagonalMove.Value.x && _diagonal.y != _lastDiagonalMove.Value.y)) continue;
//                _battles.Add(item);
//            }

//            battles = _battles;
//        }
//        var result = combat
//            ? battles.Count > 0
//            : steps.Count > 0 || battles.Count > 0;
//        return result;
//    }

//    /// <summary>
//    /// Заполнение списка ячеек, куда возможно перемещение фишки, с учетом правил
//    /// </summary>
//    /// <param name="selectedCell">Текущая ячейка с фишкой</param>
//	public void FillGoalCells(Cell selectedCell, List<Cell> steps = null, List<Cell> battles = null)
//    {
//        if (steps == null) steps = _steps;
//        if (battles == null) battles = _battles;
//        var pos = selectedCell.position;
//        if (selectedCell.currentFigure == null ? false : selectedCell.currentFigure.isKing) // дамка
//        {
//            AddKingGoal(steps, battles, pos, GoalDirection.TopRight);
//            AddKingGoal(steps, battles, pos, GoalDirection.BottomLeft);
//            AddKingGoal(steps, battles, pos, GoalDirection.BottomRight);
//            AddKingGoal(steps, battles, pos, GoalDirection.TopLeft);

//            if (battles.Count > 0)
//            {
//                steps.Clear();
//                _BattleFoundEvent(battles);
//            }
//        }
//        else // обычная шашка
//        {
//            AddGoal(battles, pos, +2, +2);
//            AddGoal(battles, pos, -2, -2);
//            AddGoal(battles, pos, +2, -2);
//            AddGoal(battles, pos, -2, +2);

//            if (battles.Count > 0)
//            {
//                steps.Clear();
//                _BattleFoundEvent(battles);
//                return;
//            }
//            AddGoal(steps, pos, -1, -1);
//            AddGoal(steps, pos, +1, -1);
//            AddGoal(steps, pos, -1, +1);
//            AddGoal(steps, pos, +1, +1);
//        }
//    }

//    public List<Cell> FindMoreBattles(Cell startCell, Cell selectedCell, bool isKing)
//    {
//        var battles = new List<Cell>();
//        var pos = selectedCell.position;
//        var diagonal = GetDiagonal(startCell, selectedCell);

//        if (isKing) // дамка
//        {
//            // снятие фишек противника
//            var dx = diagonal.x;
//            var dy = diagonal.y;
//            var addr = startCell.position;

//            while (true)
//            {
//                addr = new Vector2(addr.x + dx, addr.y + dy);
//                var cell = GetCellByPoint(addr);
//                if (cell == null) break;
//                if (cell.isEmpty() || addr == selectedCell.position) break;

//            }

//            AddKingGoal(null, battles, pos, GoalDirection.BottomLeft);
//            AddKingGoal(null, battles, pos, GoalDirection.BottomRight);
//            AddKingGoal(null, battles, pos, GoalDirection.TopLeft);
//            AddKingGoal(null, battles, pos, GoalDirection.TopRight);
//        }
//        else // обычная шашка
//        {
//            var addr = new Vector2((startCell.position.x + selectedCell.position.x) / 2,
//                                            (startCell.position.y + selectedCell.position.y) / 2);
//            var cell = GetCellByPoint(addr);
//            AddGoal(battles, pos, -2, -2);
//            AddGoal(battles, pos, +2, -2);
//            AddGoal(battles, pos, +2, +2);
//            AddGoal(battles, pos, -2, +2);
//        }

//        if (battles.Count > 0)
//        {
//            List<Cell> _battles = new List<Cell>();
//            foreach (var item in battles)
//            {
//                var _diagonal = GetDiagonal(selectedCell, item);
//                if ((_diagonal.x != diagonal.x && _diagonal.y != diagonal.y)) continue;
//                _battles.Add(item);
//            }

//            if (_battles.Count == 0)
//                return null;
//            battles = _battles;
//        }

//        return battles;
//    }

//    /// <summary>
//    /// Добавление целевого поля для дамки
//    /// </summary>
//    /// <param name="goalList">Список целей, накопительный</param>
//    /// <param name="pos">Адрес ячейки, вокруг которой ищется цель</param>
//    /// <param name="direction">Направление поиска в "глубину"</param>
//    /// <returns>true - была также найдена возможность боя</returns>
//    private void AddKingGoal(List<Cell> steps, List<Cell> battles, Vector2 pos, GoalDirection direction)
//    {
//        int dx, dy;
//        switch (direction)
//        {
//            case GoalDirection.BottomLeft: dx = dy = -1; break;
//            case GoalDirection.BottomRight: dx = +1; dy = -1; break;
//            case GoalDirection.TopRight: dx = dy = +1; break;
//            case GoalDirection.TopLeft: dx = -1; dy = +1; break;
//            default: return;
//        }
//        var source = GetCellByPoint(pos);
//        var combat = false;
//        var addr = pos;
//        while (true)
//        {
//            addr = new Vector2(addr.x + dx, addr.y + dy);
//            if (!Cell.isCorrectCell(addr)) break;
//            var cell = GetCellByPoint(addr);
//            if (cell.isEmpty()) break;
//            if (cell.State == State.Empty)
//            {
//                if (combat)
//                    battles.Add(cell);
//                else
//                    if (steps != null)
//                    steps.Add(cell);
//            }
//            else if (cell.State != source.State)
//            {
//                addr = new Vector2(addr.x + dx, addr.y + dy);
//                if (!Cell.isCorrectCell(addr)) break;
//                if (_grid.cells[(int)addr.x, (int)addr.y].isEmpty()) break;
//                cell = GetCellByPoint(addr);
//                if (cell.State == State.Empty)
//                {
//                    if (combat) break;
//                    battles.Add(GetCellByPoint(addr));
//                    combat = true;
//                }
//                else
//                    break;
//            }
//            else
//                break;
//        }
//    }

//    /// <summary>
//    /// Добавление целевого поля для шашки
//    /// </summary>
//    /// <param name="goalList">Список целей, накопительный</param>
//    /// <param name="pos">Адрес ячейки, вокруг которой ищется цель</param>
//    /// <param name="dx">Шаг поиска по горизонтали</param>
//    /// <param name="dy">Шаг поиска по вертикали</param>
//	private void AddGoal(List<Cell> goalList, Vector2 pos, int dx, int dy)
//    {
//        var addr = new Vector2(pos.x + dx, pos.y + dy);
//        var check = CheckMove(pos, addr, false);

//        if (check != MoveResult.Prohibited)
//            goalList.Add(GetCellByPoint(addr));
//    }
//}
