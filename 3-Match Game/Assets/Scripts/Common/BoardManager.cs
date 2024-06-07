using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Const;
using System;
using UnityEditor.Search;
using Unity.VisualScripting;
using System.Linq;

public class BoardManager : MonoSingleton<BoardManager>
{
    // 현재 보드의 X, Y 길이
    [SerializeField] public int width = 7;
    [SerializeField] public int height = 7;

    [SerializeField] private GameObject backGroundBlock;
    [SerializeField] private GameObject[] dots;

    [SerializeField] public float swipeThreshold = 0.5f;  // 터치 이동 최소 거리 임계값
    [SerializeField] private float speed = 6f;
    [SerializeField] private float displacement = 0.05f;

    public Transform trTileObjectPool;
    private List<Dot> tileObjectPool = new List<Dot>();

    public Dot[,] allDots { get; set; }
    private GameObject[,] allBgTiles;

    [Tooltip("우, 좌, 상, 하")]
    (int, int)[] directions = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    private void Awake()
    {
        allBgTiles = new GameObject[width, height];
        allDots = new Dot[width, height];
    }

    private void Start()
    {
        SetBoard(width, height);
    }

    private void Update()
    {
        //Debug.Log("test");
    }

    public void SetBoard(int width, int height)
    {
        this.width = width;
        this.height = height;

        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                CreateBackGroundTile(i, j);
                CreateDotTile(i, j);
            }
        }

        AllTileCheck();
    }

    private void CreateBackGroundTile(int x, int y)
    {
        Vector2 tempPosition = new Vector2(x, y);
        GameObject bgTile = Instantiate(backGroundBlock, tempPosition, Quaternion.identity);
        bgTile.transform.SetParent(transform);
        bgTile.name = $"({x}, {y})";
        allBgTiles[x, y] = bgTile;
    }

    private void CreateDotTile(int x, int y)
    {
        Vector2 tempPosition = new Vector2(x, y);
        int dotToUse = UnityEngine.Random.Range(0, dots.Length);
        GameObject objDot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
        objDot.transform.SetParent(transform);

        Dot dot = objDot.GetComponent<Dot>();
        dot.tileType = (TileType)dotToUse;
        allDots[x, y] = dot;
    }



    /*
     * 전체 타일 검사해서 이미 매칭되어 있는 상태인지 확인
     * 매칭되어 있는 상태라면 hashset에 저장하고 continue
     * 매칭되어 있지 않은 상태라면 매칭가능성체크
     * 전체 타일 체크가 끝나면 hashset에 저장된 타일들 제거후 다시 전체 타일 체크
     * hashset의 count가 0이 될때까지 반복
     */
    public void AllTileCheck()
    {
        HashSet<(int, int)> matchSet = new HashSet<(int, int)>();

        foreach (var dot in allDots)
        {
            if (BFSMatchCheck(dot.CurrentX, dot.CurrentY, allDots[dot.CurrentX, dot.CurrentY].tileType, (set) => { matchSet.AddRange(set); }))
                continue;

            if (MatchPossibilityCheck(dot.CurrentX, dot.CurrentY))
                dot.isMatchable = true;
            else
                dot.isMatchable = false;
        }

        if (matchSet.Count > 0)
            MatchTileRemove(matchSet);
    }

    private void MatchTileRemove(HashSet<(int, int)> matchSet)
    {
        // 매칭된 타일들 삭제
        foreach (var (x, y) in matchSet)
        {
            allDots[x, y].RemoveTile();
            allDots[x, y] = null;
        }

        // Key : x좌표 , Value : (x좌표에서 매칭된 타일갯수, 최소 y좌표)
        Dictionary<int, List<int>> matchTileDataList = new Dictionary<int, List<int>>();

        // 루프 돌면서 매칭된 타일의 각 x좌표 별로 채울 타일 갯수 정리
        foreach (var (xPos, yPos) in matchSet)
        {
            if (matchTileDataList.ContainsKey(xPos))
            {
                matchTileDataList[xPos].Add(yPos);
            }
            else
                matchTileDataList.Add(xPos, new List<int> { yPos });
        }

        // 정리된 리스트에서 최소한 가장 낮은 위치에 있는 y좌표를 알고 있어야 해당 위치를 기준으로 남아있는 타일을 이동시킬수 있음.

        // 남아있는 타일과 해당 타일이 이동할 위치리스트
        HashSet<(Dot, Vector2)> tileTargetList = new HashSet<(Dot, Vector2)>();

        foreach (var tileMatchData in matchTileDataList)
        {
            
            int targetY = tileMatchData.Value.Min();    // 이동할 Y좌표
            int minY = tileMatchData.Value.Min() + 1;   // Y좌표 최소값
            for (int i = minY; i < height; ++i)
            {
                if (allDots[tileMatchData.Key, i] == null)
                    continue;

                tileTargetList.Add((allDots[tileMatchData.Key, i], new Vector2(tileMatchData.Key, targetY)));
                allDots[tileMatchData.Key, targetY] = allDots[tileMatchData.Key, i];
                allDots[tileMatchData.Key, i] = null;
                allDots[tileMatchData.Key, targetY].CurrentX = tileMatchData.Key;
                allDots[tileMatchData.Key, targetY].CurrentY = targetY;
                targetY++;
            }
        }

        StartCoroutine(RunCoroutine(MoveTile(tileTargetList), () =>
        {
            // 차례대로 타일 생성
            foreach (var tileMatchData in matchTileDataList)
            {
                for (int i = 0; i < tileMatchData.Value.Count; ++i)
                {
                    CreateDotTile(tileMatchData.Key, height - 1 - i);
                }
            }

            foreach (var dot in allDots)
            {
                if (dot == null)
                {
                    Debug.Log("");
                }
            }
            // 타일 추가 된 후에 전체 타일 재검사
            AllTileCheck();
        }));
    }

    // 각 타일의 매칭 가능성을 체크해서 타일마다 매칭 가능한 방향과 매칭시 매칭되는 타일의 좌표를 저장
    public bool MatchPossibilityCheck(int x, int y)
    {
        HashSet<(int, int)> matchSet = new HashSet<(int, int)>();
        
        SwapDirection direction = SwapDirection.None;

        allDots[x, y].vaildMatchSet.Clear();

        foreach (var dir in directions)
        {
            int nX = x + dir.Item1;
            int nY = y + dir.Item2;

            // 유효한 좌표인지 확인
            if (nX < 0 || nX >= width || nY < 0 || nY >= height || !allDots[nX, nY].isMovable)
                continue;

            // 체크하기 위해 임시로 타일 정보만 스왑
            (allDots[x, y], allDots[nX, nY]) = (allDots[nX, nY], allDots[x, y]);

            // 매칭 체크
            bool isMatching = BFSMatchCheck(nX, nY, allDots[nX, nY].tileType, (set) => { matchSet = set; }) ;

            // 체크 후 원위치
            (allDots[x, y], allDots[nX, nY]) = (allDots[nX, nY], allDots[x, y]);

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

                allDots[x, y].vaildMatchSet.Add(direction, matchSet);
            }
        }

        if (allDots[x, y].vaildMatchSet.ContainsKey(direction))
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
                if (allDots[nX, nY] == null)
                {
                    Debug.Log($"Dot {nX}, {nY} is null");
                    continue;
                }
                if (!allDots[nX, nY].isMovable || allDots[nX, nY].tileType != matchType)
                    continue;

                queue.Enqueue((nX, nY));
                visited[nX, nY] = true;
            }
        }

        if (horizontalSet.Count >= 3)
        {
            matchedSet.AddRange(horizontalSet);

            isMatch = true;
        }

        if (verticalSet.Count >= 3)
        {
            matchedSet.AddRange(verticalSet);

            isMatch = true;
        }

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
    public void TileSwap(Dot startTile, float swipeAngle)
    {
        int curX = startTile.CurrentX;
        int curY = startTile.CurrentY;
        Dot targetTile = null;

        SwapDirection direction = SwapDirection.None;

        // Right Swipe
        if (swipeAngle > -45f && swipeAngle <= 45f && curX < width - 1)
        {
            targetTile = allDots[curX + 1, curY];
            direction = SwapDirection.Right;
        }
        // Up Swipe
        else if (swipeAngle > 45f && swipeAngle <= 135f && curY < height - 1)
        {
            targetTile = allDots[curX, curY + 1];
            direction = SwapDirection.Up;
        }
        // Left Swipe
        else if ((swipeAngle > 135f || swipeAngle <= -135f) && curX > 0)
        {
            targetTile = allDots[curX - 1, curY];
            direction = SwapDirection.Left;
        }
        // Down Swipe
        else if (swipeAngle > -135f && swipeAngle <= -45f && curY > 0)
        {
            targetTile = allDots[curX, curY - 1];
            direction = SwapDirection.Down;
        }

        if (targetTile == null)
            return;

        SwapTiles(startTile, targetTile, direction);
    }

    public void SwapTiles(Dot startTile, Dot targetTile, SwapDirection direction, Action<bool> onComplete = null)
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



    private IEnumerator SwapTilesCoroutine(Dot startTile, Dot targetTile, SwapDirection direction, Action<bool> onComplete = null)
    {
        startTile.SetMoving(true);
        targetTile.SetMoving(true);

        Vector2 startPosition = startTile.GetPosition();
        Vector2 targetPosition = targetTile.GetPosition();

        SwapDirection targetDirection = TargetDirection(direction);

        IEnumerator startToTarget = MoveTile(startTile, targetPosition);
        IEnumerator targetToStart = MoveTile(targetTile, startPosition);

        yield return StartCoroutine(MoveBothTiles(startToTarget, targetToStart));
        
        bool isMatch = startTile.vaildMatchSet.ContainsKey(direction) || targetTile.vaildMatchSet.ContainsKey(targetDirection);

        if (!isMatch)
        {
            startToTarget = MoveTile(startTile, startPosition);
            targetToStart = MoveTile(targetTile, targetPosition);
            yield return StartCoroutine(MoveBothTiles(startToTarget, targetToStart));
        }
        else
        {
            // 매칭 성공시 좌표정보 교환
            allDots[targetTile.CurrentX, targetTile.CurrentY] = startTile;
            allDots[startTile.CurrentX, startTile.CurrentY] = targetTile;

            switch (direction)
            {
                case SwapDirection.Right:
                    targetTile.CurrentX -= 1;
                    startTile.CurrentX += 1;
                    break;
                case SwapDirection.Left:
                    targetTile.CurrentX += 1;
                    startTile.CurrentX -= 1;
                    break;

                case SwapDirection.Up:
                    targetTile.CurrentY -= 1;
                    startTile.CurrentY += 1;
                    break;
                case SwapDirection.Down:
                    targetTile.CurrentY += 1;
                    startTile.CurrentY -= 1;
                    break;
            }

            HashSet<(int, int)> matchSet = new HashSet<(int, int)> ();

            // 매칭된 타일 합체
            if (startTile.vaildMatchSet.ContainsKey(direction))
                matchSet.AddRange(startTile.vaildMatchSet[direction]);
            if (targetTile.vaildMatchSet.ContainsKey(targetDirection))
                matchSet.AddRange(targetTile.vaildMatchSet[targetDirection]);

            // 매칭된 타일 삭제
            MatchTileRemove(matchSet);
        }

        startTile.SetMoving(false);
        targetTile.SetMoving(false);

        onComplete?.Invoke(isMatch);
    }

    private IEnumerator MoveBothTiles(IEnumerator startToTarget, IEnumerator targetToStart)
    {
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

    private IEnumerator MoveTile(Dot tile, Vector2 targetPosition)
    {
        while (Vector2.Distance(targetPosition, tile.GetPosition()) > displacement)
        {
            tile.SetPosition(Vector2.Lerp(tile.GetPosition(), targetPosition, speed * Time.deltaTime));
            yield return null;
        }

        tile.SetPosition(targetPosition);
    }

    private IEnumerator MoveTile(HashSet<(Dot, Vector2)> tileTargetList)
    {
        List<Coroutine> list = new List<Coroutine>();

        // 코루틴 시작하고 리스트에 저장
        foreach (var (tile, targetPosition) in tileTargetList)
            list.Add(StartCoroutine(MoveTile(tile, targetPosition)));

        // 리스트에 저장된 코루틴들 전부 끝날때 까지 대기
        foreach (var coroutine in list)
            yield return coroutine;
    }
}
