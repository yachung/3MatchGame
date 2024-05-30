using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Const;
using System;
using UnityEditor.Search;

public class BoardManager : MonoSingleton<BoardManager>
{
    // 현재 보드의 X, Y 길이
    [SerializeField] public int width = 7;
    [SerializeField] public int height = 7;

    [SerializeField] private GameObject backGroundBlock;
    [SerializeField] private GameObject[] dots;

    [SerializeField] public float swipeThreshold = 0.5f;  // 터치 이동 최소 거리 임계값

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
        else if (swipeAngle > 135f || swipeAngle <= -135f && curX >= 0)
        {
            targetTile = allDots[curX - 1, curY];
            direction = SwapDirection.Left;
        }
        // Down Swipe
        else if (swipeAngle > -135f && swipeAngle <= -45f && curY >= 0)
        {
            targetTile = allDots[curX, curY - 1];
            direction = SwapDirection.Down;
        }

        if (targetTile == null)
            return;

        startTile.Move(targetTile.CurrentX, targetTile.CurrentY, (isComplete) => { isNext1 = isComplete; });
        targetTile.Move(startTile.CurrentX, startTile.CurrentY, (isComplete) => { isNext2 = isComplete; });

        StartCoroutine(SwapCoroutine(() => 
        {
            isNext1 = false;
            isNext2 = false;

            // 매칭 가능한 곳이면 정보 변경하고, 불가능한 곳이면 원위치 시킴.
            if (targetTile.availableDirections.Contains(direction) || startTile.availableDirections.Contains(direction))
            {
                allDots[curX, curY] = targetTile;
                allDots[targetTile.CurrentX, targetTile.CurrentY] = startTile;

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

                AllTileCheck();
            }
            else
            {
                StartCoroutine(startTile.MoveCoroutine(curX, curY));
                StartCoroutine(targetTile.MoveCoroutine(targetTile.CurrentX, targetTile.CurrentY));
            }
        }));


        //startTile.Move(targetTile.CurrentX, targetTile.CurrentY);
        //targetTile.Move(curX, curY);


    }

    bool isNext1 = false;
    bool isNext2 = false;

    public void SwapFinish(Dot startTile, Dot targetTile)
    {
        if (isNext1 && isNext2)
        {

        }
    }

    IEnumerator SwapCoroutine(Action OnComplete)
    {
        yield return new WaitUntil(() =>
        {
            if (isNext1 && isNext2)
                return true;    
            else
                return false;
        });

        OnComplete?.Invoke();
    }

    /*
     * 
     */

    IEnumerator CoTileCheck()
    {
        yield return new WaitUntil(() =>
        {
            return true;
        });

        AllTileCheck();
    }

    // 2번 실행되는중
    public void AllTileCheck()
    {
        foreach (var dot in allDots)
        {
            if (MatchPossibilityCheck(dot.CurrentX, dot.CurrentY))
            {
                dot.isMatchable = true;

                //Color originColor = dot.GetComponent<SpriteRenderer>().color;
                //originColor.a = 0.1f;
                //dot.GetComponent<SpriteRenderer>().color = originColor;
            }
            else
                dot.isMatchable = false;
        }
    }

    public bool MatchPossibilityCheck(int x, int y)
    {
        SwapDirection direction = SwapDirection.None;

        allDots[x, y].availableDirections.Clear();

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
            bool isMatching = BFSMatchCheck(nX, nY, allDots[nX, nY].tileType);

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

                allDots[x, y].availableDirections.Add(direction);
            }
        }

        if (allDots[x, y].availableDirections.Count > 0)
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

    public bool BFSMatchCheck(int startX, int startY, TileType matchType)
    {
        bool isMatching = false;

        bool[,] visited = new bool[width, height];
        Queue<(int, int)> queue = new Queue<(int, int)>();
        HashSet<(int, int)> horizontalSet = new HashSet<(int, int)>();
        HashSet<(int, int)> verticalSet = new HashSet<(int, int)>();
        //List<(int, int)> matchedDotList = new List<(int, int)>();

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

                // IsValidMatch
                if (nX < 0 || nX >= width || nY < 0 || nY >= height || !allDots[nX, nY].isMovable || allDots[nX, nY].tileType != matchType || visited[nX, nY])
                    continue;

                queue.Enqueue((nX, nY));
                visited[nX, nY] = true;
            }
        }

        if (horizontalSet.Count >= 3)
        {
            foreach (var dot in horizontalSet)
            {
                //Color originColor = allDots[dot.Item1, dot.Item2].GetComponent<SpriteRenderer>().color;
                //originColor.a = 0.1f;
                //allDots[dot.Item1, dot.Item2].GetComponent<SpriteRenderer>().color = originColor;
            }
            isMatching = true;
        }

        if (verticalSet.Count >= 3)
        {
            foreach (var dot in verticalSet)
            {
                //Color originColor = allDots[dot.Item1, dot.Item2].GetComponent<SpriteRenderer>().color;
                //originColor.a = 0.1f;
                //allDots[dot.Item1, dot.Item2].GetComponent<SpriteRenderer>().color = originColor;
            }
            isMatching = true;
        }

        return isMatching;
    }

    public void DFS(int startX, int startY)
    {
        bool[,] visited = new bool[width, height];
        Stack<(int, int)> stack = new Stack<(int, int)>();
        List<(int, int)> matchedDotList = new List<(int, int)>();

        stack.Push((startX, startY));
        visited[startX, startY] = true;

        while (stack.Count != 0)
        {
            (int, int) current = stack.Pop();
            matchedDotList.Add(current);

            foreach (var dir in directions)
            {


                int nX = current.Item1 + dir.Item1;
                int nY = current.Item2 + dir.Item2;

                // IsValidMatch
                if (nX < 0 || nX >= width || nY < 0 || nY >= height || !allDots[nX, nY].isMovable || allDots[nX, nY].tileType != allDots[startX, startY].tileType || visited[nX, nY])
                    continue;

                stack.Push((nX, nY));
                visited[nX, nY] = true;
            }
        }

        if (matchedDotList.Count >= 3)
        {
            foreach (var dot in matchedDotList)
            {
                Debug.Log($"매칭 좌표 : {dot.Item1}, {dot.Item2}");
                //Color originColor = allDots[dot.Item1, dot.Item2].GetComponent<SpriteRenderer>().color;
                //originColor.a = 0.1f;
                //allDots[dot.Item1, dot.Item2].GetComponent<SpriteRenderer>().color = originColor;
            }
            Debug.Log("끝");
        }
        //RecursiveDfs(startX, startY, visited, matchedDotList);
    }

    private void RecursiveDfs(int startX, int startY, bool[,] visited, List<(int, int)> dotList)
    {
        int x = startX;
        int y = startY;

        visited[x, y] = true;
        dotList.Add((x, y));

        // 우, 좌, 상, 하
        foreach (var dir in directions)
        {
            int nX = x + dir.Item1;
            int nY = y + dir.Item2;

            if (nX < 0 || nX >= width || nY < 0 || nY >= height || !allDots[nX, nY].isMovable || allDots[nX, nY].tileType != allDots[startX, startY].tileType || visited[nX, nY])
            {
                if (dotList.Count >= 3)
                {
                    foreach (var dot in dotList)
                    {
                        Color originColor = allDots[dot.Item1, dot.Item2].GetComponent<SpriteRenderer>().color;

                        originColor.a = 0.1f;

                        allDots[dot.Item1, dot.Item2].GetComponent<SpriteRenderer>().color = originColor;
                    }
                }

                continue;
            }

            RecursiveDfs(nX, nY, visited, dotList);
        }
    }
}
