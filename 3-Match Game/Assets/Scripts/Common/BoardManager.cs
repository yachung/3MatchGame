using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoSingleton<BoardManager>
{
    // 현재 보드의 X, Y 길이
    [SerializeField] public int width = 7;
    [SerializeField] public int height = 7;

    [SerializeField] private GameObject backGroundBlock;

    [SerializeField] private GameObject[] dots;
    [SerializeField] public Dot[,] allDots { get; set; }

    [SerializeField] public float swipeThreshold = 0.5f;  // 터치 이동 최소 거리 임계값

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
        allDots[x, y] = objDot.GetComponent<Dot>();
    }

    // 타일 방향 계산해서 스왑
    public void TileSwap(Dot startDot, float swipeAngle)
    {
        int curX = startDot.CurrentX;
        int curY = startDot.CurrentY;

        Dot targetDot;

        // Right Swipe
        if (swipeAngle > -45f && swipeAngle <= 45f && curX < width - 1)
        {
            targetDot = allDots[curX + 1, curY];
            targetDot.CurrentX -= 1;
            startDot.CurrentX += 1;
        }
        // Up Swipe
        else if (swipeAngle > 45f && swipeAngle <= 135f && curY < height - 1)
        {
            targetDot = allDots[curX, curY + 1];
            targetDot.CurrentY -= 1;
            startDot.CurrentY += 1;
        }
        // Left Swipe
        else if (swipeAngle > 135f || swipeAngle <= -135f && curX > 0)
        {
            targetDot = allDots[curX - 1, curY];
            targetDot.CurrentX += 1;
            startDot.CurrentX -= 1;
        }
        // Down Swipe
        else if (swipeAngle > -135f && swipeAngle <= -45f && curY > 0)
        {
            targetDot = allDots[curX, curY - 1];
            targetDot.CurrentY += 1;
            startDot.CurrentY -= 1;
        }
    }

    public void TileMatchCheck(int x, int y)
    {
    }
}
