using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Const;

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

    (int, int)[] directions = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    private void Start()
    {
        allBgTiles = new GameObject[width, height];
        allDots = new Dot[width, height];
        SetBoard(width, height);
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
        int dotToUse = Random.Range(0, dots.Length);
        GameObject objDot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
        objDot.transform.SetParent(transform);
        objDot.name = $"({x}, {y})";

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
        else if (swipeAngle > 135f || swipeAngle <= -135f && curX > 0)
        {
            targetTile = allDots[curX - 1, curY];
            targetTile.CurrentX += 1;
            startTile.CurrentX -= 1;
        }
        // Down Swipe
        else if (swipeAngle > -135f && swipeAngle <= -45f && curY > 0)
        {
            targetTile = allDots[curX, curY - 1];
            targetTile.CurrentY += 1;
            startTile.CurrentY -= 1;
        }
    }

    public void TileMatchCheck(Dot startTile)
    {

    }

    public void BFS(Dot startTile)
    {
        bool[,] visited;

        visited = new bool[width, height];

        Queue<(int, int)> queue = new Queue<(int, int)>();
        queue.Enqueue((startTile.CurrentX, startTile.CurrentY));
        visited[startTile.CurrentX, startTile.CurrentY] = true;

        while (queue.Count != 0)
        {
            (int, int) current = queue.Dequeue();

            foreach (var dir in directions)
            {
                int nX = current.Item1 + dir.Item1;
                int nY = current.Item2 + dir.Item2;

                if (nX < 0 || nX >= width || nY < 0 || nY >= height || !allDots[nX, nY].isMovable || allDots[nX, nY].tileType != startTile.tileType || visited[nX, nY])
                    continue;

                allDots[nX, nY].GetComponent<SpriteRenderer>().color = Color.black;
                queue.Enqueue((nX, nY));
                visited[nX, nY] = true;
            }
        }
    }
}
