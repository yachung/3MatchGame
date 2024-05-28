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

    private void Start()
    {
        allBgTiles = new GameObject[width, height];
        allDots = new Dot[width, height];
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
        //objDot.name = $"({x}, {y})";

        Dot dot = objDot.GetComponent<Dot>();
        dot.tileType = (TileType)dotToUse;
        allDots[x, y] = dot;
    }

    // 타일 방향 계산해서 스왑
    public void TileSwap(Dot startTile, float swipeAngle)
    {
        int curX = startTile.CurrentX;
        int curY = startTile.CurrentY;

        Dot targetTile;

        // Right Swipe
        if (swipeAngle > -45f && swipeAngle <= 45f && curX < width - 1)
        {
            targetTile = allDots[curX + 1, curY];
            targetTile.CurrentX -= 1;
            startTile.CurrentX += 1;
        }
        // Up Swipe
        else if (swipeAngle > 45f && swipeAngle <= 135f && curY < height - 1)
        {
            targetTile = allDots[curX, curY + 1];
            targetTile.CurrentY -= 1;
            startTile.CurrentY += 1;
        }
        // Left Swipe
        else if (swipeAngle > 135f || swipeAngle <= -135f && curX >= 0)
        {
            targetTile = allDots[curX - 1, curY];
            targetTile.CurrentX += 1;
            startTile.CurrentX -= 1;
        }
        // Down Swipe
        else if (swipeAngle > -135f && swipeAngle <= -45f && curY >= 0)
        {
            targetTile = allDots[curX, curY - 1];
            targetTile.CurrentY += 1;
            startTile.CurrentY -= 1;
        }
    }

    public void TileMatchCheck(Dot startTile)
    {

    }

    /*
     * startX, startY 위치의 타일부터 BFS 실행
     * 
     */

    public void BFS(int startX, int startY)
    {
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
                if (nX < 0 || nX >= width || nY < 0 || nY >= height || !allDots[nX, nY].isMovable || allDots[nX, nY].tileType != allDots[startX, startY].tileType || visited[nX, nY])
                    continue;

                queue.Enqueue((nX, nY));
                visited[nX, nY] = true;
            }
        }

        if (horizontalSet.Count >= 3)
        {
            foreach (var dot in horizontalSet)
            {
                Color originColor = allDots[dot.Item1, dot.Item2].GetComponent<SpriteRenderer>().color;

                originColor.a = 0.1f;

                allDots[dot.Item1, dot.Item2].GetComponent<SpriteRenderer>().color = originColor;
            }
        }

        if (verticalSet.Count >= 3)
        {
            foreach (var dot in verticalSet)
            {
                Color originColor = allDots[dot.Item1, dot.Item2].GetComponent<SpriteRenderer>().color;

                originColor.a = 0.1f;

                allDots[dot.Item1, dot.Item2].GetComponent<SpriteRenderer>().color = originColor;
            }
        }
    }

    public void DFS(int startX, int startY)
    {
        bool[,] visited = new bool[width, height];
        Stack<(int, int)> stack = new Stack<(int, int)> ();
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

                if (dir.Item2 == 0)
                {

                }
                else
                {

                }

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
            if (dir.Item2 == 0)
            {

            }
            else
            {
                
            }
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
