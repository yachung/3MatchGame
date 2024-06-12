using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Const;
using System;
using UnityEditor.Search;
using System.Linq;
using Unity.VisualScripting;

// 전반적인 게임 로직 담당하는 매니저
public class BoardManager : MonoSingleton<BoardManager>
{
    // 현재 보드의 X, Y 길이
    [SerializeField] public int width = 7;
    [SerializeField] public int height = 7;

    [SerializeField] private GameObject backGroundTile;
    [SerializeField] private GameObject[] Tiles;
    [SerializeField] private GameObject defaultTile;

    [SerializeField] public float swipeThreshold = 0.5f;  // 터치 이동 최소 거리 임계값
    [SerializeField] public float speed = 6f;
    [SerializeField] public float displacement = 0.05f;

    public Tile[,] TileGrid { get; set; }
    private GameObject[,] BackGroundTileGrid;

    [Tooltip("우, 좌, 상, 하")]
    (int, int)[] directions = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    private void Awake()
    {
        BackGroundTileGrid = new GameObject[width, height];
        TileGrid = new Tile[width, height];
    }

    private void Start()
    {
        ObjectPoolingManager.Instance.CreatePool("Tile", defaultTile, width * height);
        SetBoard(width, height);
    }

    private void Update()
    {
        //Debug.Log("test");
    }

    private void ClearBoard(int width, int height)
    {
        Debug.LogWarning("!!!!!!!Board Clear!!!!!!!!");
        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                if (TileGrid[i, j] == null)
                    continue;

                TileGrid[i, j].RemoveTile();
            }
        }
    }

    public void RefreshBoard()
    {
        ClearBoard(width, height);

        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                //CreateBackGroundTile(i, j);
                SpawnTile(i, j);
            }
        }

        AllTileCheck();
    }


    public void SetBoard(int width, int height, bool isClear = false)
    {
        this.width = width;
        this.height = height;

        if (isClear)
            ClearBoard(width, height);

        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                //CreateBackGroundTile(i, j);
                SpawnTile(i, j);
            }
        }

        AllTileCheck();
    }

    private void CreateBackGroundTile(int x, int y)
    {
        Vector2 tempPosition = new Vector2(x, y);
        GameObject bgTile = Instantiate(backGroundTile, tempPosition, Quaternion.identity);
        bgTile.transform.SetParent(transform);
        bgTile.name = $"({x}, {y})";
        BackGroundTileGrid[x, y] = bgTile;
    }

    private void SpawnTile(int x, int y)
    {
        Vector2 tempPosition = new Vector2(x, y);
        int randomTile = UnityEngine.Random.Range(0, (int)TileType.Count);
        //GameObject objDot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
        GameObject objTile = ObjectPoolingManager.Instance.GetObject("Tile");
        objTile.transform.SetParent(transform);

        Tile tile = objTile.GetComponent<Tile>();
        tile.Initialize(tempPosition);
        tile.tileType = (TileType)randomTile;
        string colorHex = tile.tileType.GetDescription();
        tile.SetColor(colorHex);

        TileGrid[x, y] = tile;
    }

    /*
     * 전체 타일 검사해서 이미 매칭되어 있는 상태인지 확인
     * 매칭되어 있는 상태라면 hashset에 저장하고 continue
     * 매칭되어 있지 않은 상태라면 매칭가능성체크
     * 전체 타일 체크가 끝나면 hashset에 저장된 타일들 제거후 다시 전체 타일 체크
     * 매칭된 타일이 없어질때까지 반복됨
     * 매칭 가능한 타일이 없다면, 전체 보드 초기화
     */

    public void AllTileCheck()
    {
        Debug.Log("AllTileCheck Call");
        HashSet<(int, int)> matchSet = new HashSet<(int, int)>();

        foreach (var tile in TileGrid)
        {
            if (BFSMatchCheck(tile.LogicalX, tile.LogicalY, tile.tileType, (set) => { matchSet.AddRange(set); }))
                continue;

            if (MatchPossibilityCheck(tile.LogicalX, tile.LogicalY))
                tile.isMatchable = true;
            else
                tile.isMatchable = false;
        }

        if (matchSet.Count > 0)
        {
            StartCoroutine(RemoveCoroutine(matchSet));
            //MatchTileRemove(matchSet);
        }
        
        // 현재 매칭된 타일이 없고, 매칭 가능한 타일도 없다면 현재 보드를 초기화 함
        //if (matchSet.Count == 0)
        //{
        //    foreach (var tile in TileGrid)
        //        if (tile.isMatchable)
        //            return;

        //    SetBoard(width, height, isClear : true);
        //}
    }

    IEnumerator RemoveCoroutine(HashSet<(int, int)> matchSet)
    {
        foreach (var (x, y) in matchSet)
            TileGrid[x, y].TestTile();

        yield return new WaitForSeconds(2f);

        foreach (var (x, y) in matchSet)
        {
            TileGrid[x, y].MatchTile();
            TileGrid[x, y] = null;
        }

        RefillBoard();
    }

    private void MatchTileRemove(HashSet<(int, int)> matchSet)
    {
        foreach (var (x, y) in matchSet)
        {
            TileGrid[x, y].MatchTile();
            TileGrid[x, y] = null;
        }

        RefillBoard();
    }

    Coroutine refillCoroutine;

    private void RefillBoard()
    {
        HashSet<(Tile tile, Vector2 target)> tileTargetList = new HashSet<(Tile, Vector2)>();

        int refillCount = 0;
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                if (TileGrid[i, j] == null)
                    refillCount++;
                else if (refillCount > 0)
                {
                    tileTargetList.Add((TileGrid[i, j], new Vector2(i, j - refillCount)));
                    TileGrid[i, j - refillCount] = TileGrid[i, j];
                    TileGrid[i, j] = null;
                    TileGrid[i, j - refillCount].LogicalX = i;
                    TileGrid[i, j - refillCount].LogicalY = j - refillCount;
                }
            }
            refillCount = 0;
        }

        if (refillCoroutine != null)
            StopCoroutine(refillCoroutine);

        foreach (var item in tileTargetList)
            item.tile.SetMoving(true);

        refillCoroutine = StartCoroutine(RunCoroutine(MoveTileList(tileTargetList), () =>
        {
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    if (TileGrid[i, j] == null)
                    {
                        SpawnTile(i, j);
                    }
                }
            }

            foreach (var item in tileTargetList)
                item.tile.SetMoving(false);

            // 타일 추가 된 후에 전체 타일 재검사
            AllTileCheck();
        }));
    }

    // 각 타일의 매칭 가능성을 체크해서 타일마다 매칭 가능한 방향과 매칭시 매칭되는 타일의 좌표를 저장
    public bool MatchPossibilityCheck(int x, int y)
    {
        HashSet<(int, int)> matchSet = new HashSet<(int, int)>();
        
        SwapDirection direction = SwapDirection.None;

        TileGrid[x, y].vaildMatchSet.Clear();

        foreach (var dir in directions)
        {
            int nX = x + dir.Item1;
            int nY = y + dir.Item2;

            // 유효한 좌표인지 확인
            if (nX < 0 || nX >= width || nY < 0 || nY >= height)
                continue;

            if (TileGrid[nX, nY] == null)
                continue;

            // 이동할수 없는 타일은 매칭할수 없으므로 스킵
            if (!TileGrid[nX, nY].isMovable)
                continue;

            // 체크하기 위해 임시로 타일 정보만 스왑
            (TileGrid[x, y], TileGrid[nX, nY]) = (TileGrid[nX, nY], TileGrid[x, y]);

            // 매칭 체크
            bool isMatching = BFSMatchCheck(nX, nY, TileGrid[nX, nY].tileType, (set) => { matchSet = set; }) ;

            // 체크 후 원위치
            (TileGrid[x, y], TileGrid[nX, nY]) = (TileGrid[nX, nY], TileGrid[x, y]);

            if (isMatching)
            {
                switch (dir)
                {
                    case (1, 0):
                        direction = SwapDirection.Right;
                        break;
                    case (-1, 0):
                        direction = SwapDirection.Left;
                        break;
                    case (0, 1):
                        direction = SwapDirection.Up;
                        break;
                    case (0, -1):
                        direction = SwapDirection.Down;
                        break;
                }

                TileGrid[x, y].vaildMatchSet.Add(direction, matchSet);
            }
        }

        if (TileGrid[x, y].vaildMatchSet.ContainsKey(direction))
            return true;
        else
            return false;
    }

    /* 24.05.29
     * 타일이 이동할때마다 CallBack으로 BFS를 돌려서 매칭체크를 했지만
     * 이런 식으로 타일이 이동할 때만 매칭체크를 한다면 
     * 전체 보드에서 매칭할 타일이 없는 경우를 체크하지 못함
     * 그렇다면 결국 타일이 이동할때 마다 전체 보드의 각 타일마다 매칭 가능성 여부를 체크하는 함수가 돌아야 할 것 같다.
     */

    private bool BFSMatchCheck(int startX, int startY, TileType matchType, Action<HashSet<(int, int)>> checkResult = null)
    {
        //Debug.Log("BFSMatchCheck Call");
        bool isMatch = false;

        bool[,] visited = new bool[width, height];
        Queue<(int, int)> queue = new Queue<(int, int)>();
        HashSet<(int, int)> horizontalSet = new HashSet<(int, int)>();
        HashSet<(int, int)> verticalSet = new HashSet<(int, int)>();
        HashSet<(int, int)> matchedSet = new HashSet<(int, int)>();

        queue.Enqueue((startX, startY));
        visited[startX, startY] = true;

        horizontalSet.Add((startX, startY));
        verticalSet.Add((startX, startY));

        while (queue.Count != 0)
        {
            (int, int) current = queue.Dequeue();

            if (startX == current.Item1)
                verticalSet.Add(current);
            else if (startY == current.Item2)
                horizontalSet.Add(current);

            foreach (var dir in directions)
            {
                int nX = current.Item1 + dir.Item1;
                int nY = current.Item2 + dir.Item2;

                if (nX < 0 || nX >= width || nY < 0 || nY >= height || visited[nX, nY])
                    continue;

                // IsValidMatch
                if (TileGrid[nX, nY] == null)
                {
                    Debug.Log($"Tile {nX}, {nY} is null");
                    continue;
                }
                if (!TileGrid[nX, nY].isMovable || TileGrid[nX, nY].tileType != matchType)
                    continue;

                queue.Enqueue((nX, nY));
                visited[nX, nY] = true;
            }
        }

        if (horizontalSet.Count >= 3)
            matchedSet.AddRange(horizontalSet);

        if (verticalSet.Count >= 3)
            matchedSet.AddRange(verticalSet);

        if (matchedSet.Count > 0)
            isMatch = true;

        checkResult?.Invoke(matchedSet);

        return isMatch;
    }

    #region SwapFunction
    // 스왑방향을 타겟시점에서 보는 방향으로 바꾸는 함수 -> 방향을 반대로
    private SwapDirection TargetDirection(SwapDirection direction)
    {
        switch (direction)
        {
            case SwapDirection.Right:
                direction = SwapDirection.Left;
                break;
            case SwapDirection.Left:
                direction = SwapDirection.Right;
                break;
            case SwapDirection.Up:
                direction = SwapDirection.Down;
                break;
            case SwapDirection.Down:
                direction = SwapDirection.Up;
                break;
        }

        return direction;
    }

    // 타일 방향 계산해서 스왑
    public void TileSwap(Tile startTile, float swipeAngle)
    {
        Debug.Log("TileSwap Call");
        int curX = startTile.LogicalX;
        int curY = startTile.LogicalY;
        Tile targetTile = null;

        SwapDirection direction = SwapDirection.None;
        
        // Right Swipe
        if (swipeAngle > -45f && swipeAngle <= 45f && curX < width - 1)
        {
            targetTile = TileGrid[curX + 1, curY];
            direction = SwapDirection.Right;
        }
        // Up Swipe
        else if (swipeAngle > 45f && swipeAngle <= 135f && curY < height - 1)
        {
            targetTile = TileGrid[curX, curY + 1];
            direction = SwapDirection.Up;
        }
        // Left Swipe
        else if ((swipeAngle > 135f || swipeAngle <= -135f) && curX > 0)
        {
            targetTile = TileGrid[curX - 1, curY];
            direction = SwapDirection.Left;
        }
        // Down Swipe
        else if (swipeAngle > -135f && swipeAngle <= -45f && curY > 0)
        {
            targetTile = TileGrid[curX, curY - 1];
            direction = SwapDirection.Down;
        }

        if (targetTile == null)
            return;

        SwapTiles(startTile, targetTile, direction);
    }

    public void SwapTiles(Tile startTile, Tile targetTile, SwapDirection direction, Action<bool> onComplete = null)
    {
        if (startTile.IsMoving || targetTile.IsMoving)
        {
            Debug.Log($"startTile {startTile.GetPosition()} is {startTile.IsMoving}");
            Debug.Log($"targetTile {targetTile.GetPosition()} is {targetTile.IsMoving}");
            onComplete?.Invoke(false);
            return;
        }

        StartCoroutine(SwapTilesCoroutine(startTile, targetTile, direction, onComplete));
    }

    private IEnumerator SwapTilesCoroutine(Tile startTile, Tile targetTile, SwapDirection direction, Action<bool> onComplete = null)
    {
        startTile.SetMoving(true);
        targetTile.SetMoving(true);

        Vector2 startPosition = startTile.GetPosition();
        Vector2 targetPosition = targetTile.GetPosition();

        SwapDirection targetDirection = TargetDirection(direction);

        IEnumerator startToTarget = startTile.MoveCoroutine(targetPosition);
        IEnumerator targetToStart = targetTile.MoveCoroutine(startPosition);

        IEnumerator rollBackStart = startTile.MoveCoroutine(startPosition);
        IEnumerator rollBackTarget = targetTile.MoveCoroutine(targetPosition);

        bool isMatch = startTile.vaildMatchSet.ContainsKey(direction) || targetTile.vaildMatchSet.ContainsKey(targetDirection);

        if (isMatch)
        {
            // 매칭 성공시 좌표정보 교환
            TileGrid[targetTile.LogicalX, targetTile.LogicalY] = startTile;
            TileGrid[startTile.LogicalX, startTile.LogicalY] = targetTile;

            switch (direction)
            {
                case SwapDirection.Right:
                    targetTile.LogicalX -= 1;
                    startTile.LogicalX += 1;
                    break;
                case SwapDirection.Left:
                    targetTile.LogicalX += 1;
                    startTile.LogicalX -= 1;
                    break;
                case SwapDirection.Up:
                    targetTile.LogicalY -= 1;
                    startTile.LogicalY += 1;
                    break;
                case SwapDirection.Down:
                    targetTile.LogicalY += 1;
                    startTile.LogicalY -= 1;
                    break;
            }
        }

        yield return StartCoroutine(MoveBothTiles(startToTarget, targetToStart));

        // 첫 스왑 끝난 후 매칭 불가능하면 원상복귀, 매칭 가능하면 매칭타일 삭제 함수 호출
        if (!isMatch)
        {
            yield return StartCoroutine(MoveBothTiles(rollBackStart, rollBackTarget));
        }
        else
        {
            HashSet<(int, int)> matchSet = new HashSet<(int, int)> ();

            // 매칭된 타일 합체
            if (startTile.vaildMatchSet.ContainsKey(direction))
                matchSet.AddRange(startTile.vaildMatchSet[direction]);
            if (targetTile.vaildMatchSet.ContainsKey(targetDirection))
                matchSet.AddRange(targetTile.vaildMatchSet[targetDirection]);

            // 매칭된 타일 삭제
            //MatchTileRemove(matchSet);
            yield return StartCoroutine(RemoveCoroutine(matchSet));
        }

        startTile.SetMoving(false);
        targetTile.SetMoving(false);

        onComplete?.Invoke(isMatch);
    }

    private IEnumerator MoveBothTiles(IEnumerator startToTarget, IEnumerator targetToStart)
    {
        Debug.Log("MoveBothTiles Call");
        bool tile1Finished = false;
        bool tile2Finished = false;

        StartCoroutine(RunCoroutine(startToTarget, () => tile1Finished = true));
        StartCoroutine(RunCoroutine(targetToStart, () => tile2Finished = true));

        yield return new WaitUntil(() => tile1Finished && tile2Finished);
    }

    #endregion

    private IEnumerator RunCoroutine(IEnumerator coroutine, Action onComplete)
    {
        yield return StartCoroutine(coroutine);
        onComplete?.Invoke();
    }

    // 타일 이동 함수
    //private IEnumerator MoveTile(Tile tile, Vector2 targetPosition)
    //{
    //    tile.SetMoving(true);

    //    while (Vector2.Distance(targetPosition, tile.GetPosition()) > displacement)
    //    {
    //        tile.SetPosition(Vector2.Lerp(tile.GetPosition(), targetPosition, speed * Time.deltaTime));
    //        yield return null;
    //    }

    //    tile.SetPosition(targetPosition);

    //    tile.SetMoving(false);
    //}

    // 타일 여러개 동시 이동을 위한 함수
    private IEnumerator MoveTileList(HashSet<(Tile, Vector2)> tileTargetList)
    {
        Debug.Log("MoveTileList Call");

        List<Coroutine> tileCoroutineList = new List<Coroutine>();

        // 코루틴 시작하고 리스트에 저장
        foreach (var (tile, targetPosition) in tileTargetList)
            tileCoroutineList.Add(StartCoroutine(tile.MoveCoroutineInvoke(targetPosition)));

        // 리스트에 저장된 코루틴들 전부 끝날때 까지 대기
        foreach (var coroutine in tileCoroutineList)
            yield return coroutine;
    }

    //private Coroutine moveTileCoroutine;
    //private List<Coroutine> tileCoroutineList = new List<Coroutine>();

    //private IEnumerator MoveTileList(HashSet<(Tile, Vector2)> tileTargetList)
    //{
    //    Debug.Log("MoveTileList Call");

    //    // 기존에 실행 중이던 코루틴이 있으면 중지
    //    if (moveTileCoroutine != null)
    //    {
    //        StopCoroutine(moveTileCoroutine);
    //        moveTileCoroutine = null;
    //    }

    //    // 기존에 실행 중이던 타일 이동 코루틴들도 모두 중지
    //    foreach (var coroutine in tileCoroutineList)
    //    {
    //        if (coroutine != null)
    //        {
    //            StopCoroutine(coroutine);
    //        }
    //    }

    //    // 새로운 타일 이동 코루틴 리스트 초기화
    //    tileCoroutineList.Clear();

    //    // 새로운 코루틴 실행 및 관리
    //    moveTileCoroutine = StartCoroutine(RunTileCoroutines(tileTargetList));

    //    yield return moveTileCoroutine;
    //}

    //private IEnumerator RunTileCoroutines(HashSet<(Tile, Vector2)> tileTargetList)
    //{
    //    // 코루틴 시작하고 리스트에 저장
    //    foreach (var (tile, targetPosition) in tileTargetList)
    //    {
    //        Coroutine tileCoroutine = StartCoroutine(tile.MoveCoroutine(targetPosition));
    //        tileCoroutineList.Add(tileCoroutine);
    //    }

    //    // 리스트에 저장된 코루틴들 전부 끝날때 까지 대기
    //    foreach (var coroutine in tileCoroutineList)
    //    {
    //        yield return coroutine;
    //    }

    //    // 모든 코루틴 종료 후, 관리 변수 초기화
    //    moveTileCoroutine = null;
    //    tileCoroutineList.Clear();
    //}
}
