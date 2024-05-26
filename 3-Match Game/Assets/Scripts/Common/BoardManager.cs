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
    [SerializeField] public GameObject[,] allDots { get; set; }

    [SerializeField] public float swipeThreshold = 0.5f;  // 터치 이동 최소 거리 임계값

    private GameObject[,] allTiles;

    (int, int)[] directions = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    private void Start()
    {
        allTiles = new GameObject[width, height];
        allDots = new GameObject[width, height];
        SetBoard(width, height);
    }

    public void SetBoard(int xDim, int yDim)
    {
        this.width = xDim;
        this.height = yDim;

        //for (int j = yDim / 2; j >= -yDim / 2; --j)
        //{
        //    for (int i = -xDim / 2; i <= xDim / 2; ++i)
        //    {
        //        GameObject bgBlock = Instantiate(backGroundBlock);
        //        bgBlock.transform.position = new Vector3(i + 0.1f * i, j + 0.1f * j, 0);
        //        bgBlock.transform.SetParent(this.transform);
        //    }
        //}

        for (int j = 0; j < yDim; ++j)
        {
            for (int i = 0; i < xDim; ++i)
            {
                Vector2 tempPosition = new Vector2(i, j);
                GameObject bgTile = Instantiate(backGroundBlock, tempPosition, Quaternion.identity);
                bgTile.transform.SetParent(this.transform);
                bgTile.name = $"({i}, {j})";
                allTiles[i, j] = bgTile;

                int dotToUse = Random.Range(0, dots.Length);
                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                dot.transform.SetParent(transform);
                dot.name = $"({i}, {j})";
                allDots[i, j] = dot;
            }
        }

    }


}
