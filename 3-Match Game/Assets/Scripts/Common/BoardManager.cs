using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoSingleton<BoardManager>
{
    // 현재 보드의 X, Y 길이
    [SerializeField] private int xDim = 7;
    [SerializeField] private int yDim = 7;

    [SerializeField] private GameObject backGroundBlock;

    private void Start()
    {
        SetBoard(xDim, yDim);
    }

    public void SetBoard(int xDim, int yDim)
    {
        this.xDim = xDim;
        this.yDim = yDim;

        for (int j = yDim / 2; j >= -yDim / 2; --j)
        {
            for (int i = -xDim / 2; i <= xDim / 2; ++i)
            {
                GameObject bgBlock = Instantiate(backGroundBlock);
                bgBlock.transform.position = new Vector3(i + 0.1f * i, j + 0.1f * j, 0);
                bgBlock.transform.SetParent(this.transform);
            }
        }
    }
}
